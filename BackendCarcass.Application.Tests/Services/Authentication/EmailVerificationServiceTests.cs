using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Email;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.EmailVerifications;
using BackendCarcass.Domain.Users;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.Services.Authentication;

// Only the latest code of a user and purpose is valid, it expires, it survives a limited number of wrong guesses,
// it cannot be requested again within the cooldown, and the e-mail carries the code and (where there is a page
// for it) a link with a separate token
public sealed class EmailVerificationServiceTests : IDisposable
{
    private readonly CarcassSqliteDatabase _db = new();
    private readonly CapturingEmailSender _sender = new();
    private readonly EmailVerificationService _service;
    private readonly AppUser _user;

    public EmailVerificationServiceTests()
    {
        User user = _db.AddUser();
        _user = new AppUser(user.UserName, user.FirstName, user.LastName) { Id = user.UsrId, Email = user.Email };
        _service = new EmailVerificationService(_db.Context, _sender,
            Options.Create(new IdentitySettings { FrontendBaseUrl = "https://app.test/" }),
            Options.Create(new EmailSettings { SenderName = "App" }),
            NullLogger<EmailVerificationService>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task Sending_stores_a_hashed_row_and_mails_the_code_and_the_link()
    {
        Result result = await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);

        Assert.True(result.IsSuccess);
        EmailVerification row = Assert.Single(_db.Context.EmailVerifications);
        Assert.Equal(_user.Id, row.UserId);
        Assert.Equal(EEmailVerificationPurpose.EmailConfirmation, row.Purpose);
        Assert.Null(row.UsedAt);
        Assert.Equal(0, row.Attempts);

        (string toEmail, _, string subject, string body) = _sender.Last;
        Assert.Equal(_user.Email, toEmail);
        Assert.StartsWith("App: ", subject, StringComparison.Ordinal);
        string code = ExtractCode(body);
        Assert.DoesNotContain(code, row.CodeHash, StringComparison.Ordinal);
        Assert.Contains("https://app.test/confirm-email?userId=" + _user.Id + "&token=", body,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_password_change_mail_has_no_link()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.PasswordChange);

        Assert.DoesNotContain("https://", _sender.Last.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_password_reset_link_points_to_the_reset_page()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.PasswordReset);

        Assert.Contains("https://app.test/reset-password?userId=", _sender.Last.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Without_a_frontend_url_only_the_code_is_mailed()
    {
        var service = new EmailVerificationService(_db.Context, _sender, Options.Create(new IdentitySettings()),
            Options.Create(new EmailSettings()), NullLogger<EmailVerificationService>.Instance);

        await service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);

        Assert.DoesNotContain("token=", _sender.Last.Body, StringComparison.Ordinal);
        Assert.Matches("^[0-9]{6}$", ExtractCode(_sender.Last.Body));
    }

    [Fact]
    public async Task A_failing_mail_server_yields_a_controlled_error_instead_of_an_exception()
    {
        var service = new EmailVerificationService(_db.Context, new ThrowingEmailSender(),
            Options.Create(new IdentitySettings()), Options.Create(new EmailSettings()),
            NullLogger<EmailVerificationService>.Instance);

        Result result = await service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);

        Assert.Equal(AuthenticationApiErrors.EmailSendingFailed, result.Error);
    }

    private sealed class ThrowingEmailSender : IEmailSender
    {
        public Task SendAsync(string toEmail, string toName, string subject, string body,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("SMTP is down");
        }
    }

    [Fact]
    public async Task The_right_code_verifies_once_only()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);
        string code = ExtractCode(_sender.Last.Body);

        Result first = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation, code);
        Result second = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation, code);

        Assert.True(first.IsSuccess);
        Assert.Equal(AuthenticationApiErrors.InvalidOrExpiredCode, second.Error);
        Assert.NotNull(_db.Context.EmailVerifications.Single().UsedAt);
    }

    [Fact]
    public async Task A_code_sent_for_one_purpose_does_not_verify_another()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);
        string code = ExtractCode(_sender.Last.Body);

        Result result = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.PasswordReset, code);

        Assert.Equal(AuthenticationApiErrors.InvalidOrExpiredCode, result.Error);
    }

    [Fact]
    public async Task Wrong_guesses_are_counted_and_the_fifth_one_locks_the_code()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);
        string code = ExtractCode(_sender.Last.Body);
        string wrongCode = code == "000000" ? "000001" : "000000";

        for (int i = 0; i < 4; i++)
        {
            Result wrong = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
                wrongCode);
            Assert.Equal(AuthenticationApiErrors.InvalidOrExpiredCode, wrong.Error);
        }

        Result fifth = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
            wrongCode);
        Result rightButLate = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
            code);

        Assert.Equal(AuthenticationApiErrors.TooManyCodeAttempts, fifth.Error);
        Assert.Equal(AuthenticationApiErrors.TooManyCodeAttempts, rightButLate.Error);
        Assert.Equal(5, _db.Context.EmailVerifications.Single().Attempts);
    }

    [Fact]
    public async Task An_expired_code_is_rejected()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);
        string code = ExtractCode(_sender.Last.Body);
        EmailVerification row = _db.Context.EmailVerifications.Single();
        row.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await _db.Context.SaveChangesAsync();

        Result result = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation, code);

        Assert.Equal(AuthenticationApiErrors.InvalidOrExpiredCode, result.Error);
    }

    [Fact]
    public async Task A_new_code_cannot_be_requested_within_the_cooldown()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);

        Result again = await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);

        Assert.Equal(AuthenticationApiErrors.CodeResendTooSoon, again.Error);
        Assert.Single(_sender.Sent);
    }

    [Fact]
    public async Task After_the_cooldown_a_new_code_replaces_the_old_one()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);
        string oldCode = ExtractCode(_sender.Last.Body);
        EmailVerification row = _db.Context.EmailVerifications.Single();
        row.CreatedAt = DateTime.UtcNow.AddMinutes(-2);
        await _db.Context.SaveChangesAsync();

        Result again = await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);
        string newCode = ExtractCode(_sender.Last.Body);

        Assert.True(again.IsSuccess);
        Assert.Single(_db.Context.EmailVerifications);
        Result oldResult = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
            oldCode);
        Result newResult = await _service.VerifyCodeAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
            newCode);
        Assert.True(oldCode == newCode || oldResult.IsFailure);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public async Task The_link_token_verifies_and_a_wrong_one_does_not()
    {
        await _service.SendAsync(_user, EEmailVerificationPurpose.EmailConfirmation);
        string linkToken = ExtractLinkToken(_sender.Last.Body);

        Result wrong = await _service.VerifyLinkAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
            "not-the-token");
        Result right = await _service.VerifyLinkAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
            linkToken);
        Result reused = await _service.VerifyLinkAsync(_user.Id, EEmailVerificationPurpose.EmailConfirmation,
            linkToken);

        Assert.Equal(AuthenticationApiErrors.InvalidConfirmationLink, wrong.Error);
        Assert.True(right.IsSuccess);
        Assert.Equal(AuthenticationApiErrors.InvalidConfirmationLink, reused.Error);
    }

    [Fact]
    public async Task A_user_without_an_email_gets_nothing()
    {
        var user = new AppUser("noemail", "No", "Email") { Id = _user.Id };

        Result result = await _service.SendAsync(user, EEmailVerificationPurpose.EmailConfirmation);

        Assert.Equal(AuthenticationApiErrors.InvalidEmail, result.Error);
        Assert.Empty(_sender.Sent);
    }

    private static string ExtractCode(string body)
    {
        //the body uses the platform line ending, so the line may end with a carriage return before the newline
        return Regex.Match(body, "(?m)^([0-9]{6})\r?$").Groups[1].Value;
    }

    private static string ExtractLinkToken(string body)
    {
        return Regex.Match(body, "token=([A-Za-z0-9_-]+)").Groups[1].Value;
    }
}
