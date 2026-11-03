using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Email;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Domain.EmailVerifications;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

//ელფოსტაზე კოდის და ბმულის გაგზავნა და მათი შემოწმება.
//ერთდროულად მოქმედია მხოლოდ ბოლოს გაგზავნილი კოდი/ბმული
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class EmailVerificationService : IScopeServiceCarcassApplication
{
    public const int CodeLifetimeMinutes = 15;
    public const int MaxAttempts = 5;
    public const int ResendCooldownSeconds = 60;

    private readonly ICarcassApplicationDbContext _ctx;
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;
    private readonly IdentitySettings _identitySettings;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(ICarcassApplicationDbContext ctx, IEmailSender emailSender,
        IOptions<IdentitySettings> identitySettings, IOptions<EmailSettings> emailSettings,
        ILogger<EmailVerificationService> logger)
    {
        _ctx = ctx;
        _emailSender = emailSender;
        _identitySettings = identitySettings.Value;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<Result> SendAsync(AppUser user, EEmailVerificationPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        if (user.Email is null)
        {
            return Result.Failure(AuthenticationApiErrors.InvalidEmail);
        }

        DateTime now = DateTime.UtcNow;
        EmailVerification? last = await LatestAsync(user.Id, purpose, cancellationToken);
        if (last is not null && last.UsedAt is null && last.CreatedAt > now.AddSeconds(-ResendCooldownSeconds))
        {
            return Result.Failure(AuthenticationApiErrors.CodeResendTooSoon);
        }

        //ახალი კოდის გაგზავნისას ძველი ჩანაწერები აღარ გვჭირდება
        await _ctx.EmailVerifications.Where(v => v.UserId == user.Id && v.Purpose == purpose)
            .ExecuteDeleteAsync(cancellationToken);

        string code = TokenHashing.CreateNumericCode();
        string linkToken = TokenHashing.CreateRandomToken();
        _ctx.EmailVerifications.Add(new EmailVerification
        {
            UserId = user.Id,
            Purpose = purpose,
            CodeHash = TokenHashing.Sha256Hex(code),
            LinkTokenHash = TokenHashing.Sha256Hex(linkToken),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(CodeLifetimeMinutes),
            Attempts = 0
        });
        await _ctx.SaveChangesAsync(cancellationToken);

        (string subject, string body) = ComposeEmail(user, purpose, code, linkToken);
        try
        {
            await _emailSender.SendAsync(user.Email, user.FullName, subject, body, cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            //SMTP სერვერის პრობლემა მომხმარებლის ბრალი არ არის: გასაგები შეცდომა ბრუნდება და არა 500
            _logger.LogError(e, "Verification e-mail could not be sent to user {UserId}", user.Id);
            return Result.Failure(AuthenticationApiErrors.EmailSendingFailed);
        }

        return Result.Success();
    }

    public async Task<Result> VerifyCodeAsync(int userId, EEmailVerificationPurpose purpose, string? code,
        CancellationToken cancellationToken = default)
    {
        EmailVerification? verification = await LatestAsync(userId, purpose, cancellationToken);
        DateTime now = DateTime.UtcNow;
        if (verification is null || verification.UsedAt is not null || verification.ExpiresAt <= now)
        {
            return Result.Failure(AuthenticationApiErrors.InvalidOrExpiredCode);
        }

        if (verification.Attempts >= MaxAttempts)
        {
            return Result.Failure(AuthenticationApiErrors.TooManyCodeAttempts);
        }

        verification.Attempts++;
        bool matches = !string.IsNullOrWhiteSpace(code) &&
                       TokenHashing.HashMatches(code.Trim(), verification.CodeHash);
        if (matches)
        {
            verification.UsedAt = now;
        }

        await _ctx.SaveChangesAsync(cancellationToken);

        if (matches)
        {
            return Result.Success();
        }

        return Result.Failure(verification.Attempts >= MaxAttempts
            ? AuthenticationApiErrors.TooManyCodeAttempts
            : AuthenticationApiErrors.InvalidOrExpiredCode);
    }

    public async Task<Result> VerifyLinkAsync(int userId, EEmailVerificationPurpose purpose, string? linkToken,
        CancellationToken cancellationToken = default)
    {
        EmailVerification? verification = await LatestAsync(userId, purpose, cancellationToken);
        DateTime now = DateTime.UtcNow;
        if (verification is null || verification.UsedAt is not null || verification.ExpiresAt <= now ||
            string.IsNullOrWhiteSpace(linkToken) ||
            !TokenHashing.HashMatches(linkToken.Trim(), verification.LinkTokenHash))
        {
            return Result.Failure(AuthenticationApiErrors.InvalidConfirmationLink);
        }

        verification.UsedAt = now;
        await _ctx.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private Task<EmailVerification?> LatestAsync(int userId, EEmailVerificationPurpose purpose,
        CancellationToken cancellationToken)
    {
        return _ctx.EmailVerifications.Where(v => v.UserId == userId && v.Purpose == purpose)
            .OrderByDescending(v => v.EvfId).FirstOrDefaultAsync(cancellationToken);
    }

    private (string Subject, string Body) ComposeEmail(AppUser user, EEmailVerificationPurpose purpose, string code,
        string linkToken)
    {
        string subjectPrefix = string.IsNullOrWhiteSpace(_emailSettings.SenderName)
            ? string.Empty
            : _emailSettings.SenderName + ": ";

        (string subject, string intro) = purpose switch
        {
            EEmailVerificationPurpose.EmailConfirmation => ("ელფოსტის დადასტურება",
                "თქვენი ელფოსტის მისამართის დასადასტურებლად შეიყვანეთ ეს კოდი:"),
            EEmailVerificationPurpose.PasswordChange => ("პაროლის შეცვლა",
                "პაროლის შესაცვლელად შეიყვანეთ ეს კოდი:"),
            EEmailVerificationPurpose.PasswordReset => ("პაროლის აღდგენა",
                "პაროლის აღსადგენად შეიყვანეთ ეს კოდი:"),
            _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, null)
        };

        var body = new StringBuilder();
        body.Append("გამარჯობა, ").Append(user.FullName).AppendLine("!").AppendLine();
        body.AppendLine(intro).AppendLine();
        body.AppendLine(code).AppendLine();
        body.Append("კოდი მოქმედებს ").Append(CodeLifetimeMinutes.ToString(CultureInfo.InvariantCulture))
            .AppendLine(" წუთის განმავლობაში.");

        string? link = BuildLink(purpose, user.Id, linkToken);
        if (link is not null)
        {
            body.AppendLine().AppendLine("კოდის ნაცვლად შეგიძლიათ გახსნათ ეს ბმული:").AppendLine(link);
        }

        body.AppendLine().AppendLine("თუ ეს მოთხოვნა თქვენ არ გაგიკეთებიათ, უბრალოდ უგულებელყავით ეს წერილი.");

        return (subjectPrefix + subject, body.ToString());
    }

    //ბმული ფრონტის გვერდზე, რომელიც ბმულიდან წაკითხულ მონაცემებს სერვერს გადასცემს
    private string? BuildLink(EEmailVerificationPurpose purpose, int userId, string linkToken)
    {
        string? baseUrl = _identitySettings.FrontendBaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        string? path = purpose switch
        {
            EEmailVerificationPurpose.EmailConfirmation => "/confirm-email",
            EEmailVerificationPurpose.PasswordReset => "/reset-password",
            _ => null
        };

        return path is null
            ? null
            : baseUrl + path + "?userId=" + userId.ToString(CultureInfo.InvariantCulture) + "&token=" +
              Uri.EscapeDataString(linkToken);
    }
}
