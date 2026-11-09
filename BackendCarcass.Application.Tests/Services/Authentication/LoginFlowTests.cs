using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Responses;
using Moq;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.Services.Authentication;

// The sign-in steps are decided apart from what the user finally gets: with the default implementations every
// NextStep yields the LoginResult the apps always got (JWT and refresh token, or a pending JWT), while another
// host can plug in its own session (ILoginTokenIssuer) and pending state (IPendingLoginStore); sign-in by e-mail
// is off unless configured, and an extra check (ILoginCheck) can refuse a user whose password is right
public sealed class LoginFlowTests
{
    private static readonly Error Deactivated = Error.Problem("UserIsDeactivated", "ანგარიში დეაქტივირებულია");

    [Fact]
    public async Task Default_full_login_returns_an_access_token_a_refresh_token_roles_and_app_claims()
    {
        using var h = new LoginServicesHarness();

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.True(result.IsSuccess);
        LoginResult login = result.Value;
        Assert.Equal(ELoginNextStep.None, login.NextStep);
        Assert.False(string.IsNullOrEmpty(login.Token));
        Assert.True(h.Tokens.ValidatePendingToken(login.Token).IsFailure);
        Assert.False(string.IsNullOrEmpty(login.RefreshToken));
        Assert.Equal(1, h.RefreshTokenRows);
        Assert.Equal(["Editor"], login.Roles);
        Assert.Equal(["ManageUsers"], login.AppClaims);
        Assert.Empty(h.Sender.Sent);

        LoginResponse response = login.ToLoginResponse();
        Assert.Equal(login.Token, response.Token);
        Assert.Equal(login.RefreshToken, response.RefreshToken);
        Assert.Equal(ELoginNextStep.None, response.NextStep);
    }

    [Fact]
    public async Task Default_unconfirmed_email_gives_a_pending_token_for_confirmation_and_mails_a_code()
    {
        using var h = new LoginServicesHarness(emailConfirmed: false);

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.Equal(ELoginNextStep.ConfirmEmail, result.Value.NextStep);
        Assert.Null(result.Value.RefreshToken);
        Assert.Empty(result.Value.Roles);
        Assert.Equal(new PendingTokenData(h.User.Id, ELoginNextStep.ConfirmEmail, false, true),
            h.Tokens.ValidatePendingToken(result.Value.Token).Value);
        Assert.Single(h.Sender.Sent);
        Assert.Equal(0, h.RefreshTokenRows);
    }

    [Fact]
    public async Task Default_unconfirmed_email_and_weak_password_carries_the_password_change_flag()
    {
        using var h = new LoginServicesHarness(emailConfirmed: false);

        Result<LoginResult> result =
            await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.WeakPassword);

        Assert.Equal(new PendingTokenData(h.User.Id, ELoginNextStep.ConfirmEmail, true, true),
            h.Tokens.ValidatePendingToken(result.Value.Token).Value);
    }

    [Fact]
    public async Task Default_weak_password_gives_a_pending_token_for_the_password_change_and_mails_a_code()
    {
        using var h = new LoginServicesHarness();

        Result<LoginResult> result =
            await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.WeakPassword);

        Assert.Equal(ELoginNextStep.ChangePassword, result.Value.NextStep);
        Assert.Equal(new PendingTokenData(h.User.Id, ELoginNextStep.ChangePassword, false, true),
            h.Tokens.ValidatePendingToken(result.Value.Token).Value);
        Assert.Single(h.Sender.Sent);
        Assert.Equal(0, h.RefreshTokenRows);
    }

    [Fact]
    public async Task Default_pending_steps_end_with_a_full_login()
    {
        using var h = new LoginServicesHarness(emailConfirmed: false);
        Result<LoginResult> login =
            await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.WeakPassword);

        Result<LoginResult> confirmed = await h.CreatePendingLoginService()
            .ConfirmEmailByCodeAsync(login.Value.Token, h.LastCode());

        Assert.True(h.User.EmailConfirmed);
        Assert.Equal(ELoginNextStep.ChangePassword, confirmed.Value.NextStep);
        Assert.Equal(new PendingTokenData(h.User.Id, ELoginNextStep.ChangePassword, false, false),
            h.Tokens.ValidatePendingToken(confirmed.Value.Token).Value);

        Result<LoginResult> changed = await h.CreatePendingLoginService()
            .ChangeRequiredPasswordAsync(confirmed.Value.Token, null, LoginServicesHarness.NewPassword);

        Assert.Equal(ELoginNextStep.None, changed.Value.NextStep);
        Assert.False(string.IsNullOrEmpty(changed.Value.RefreshToken));
        Assert.Equal(1, h.RefreshTokenRows);
    }

    [Fact]
    public async Task A_pending_token_of_another_step_is_refused()
    {
        using var h = new LoginServicesHarness();
        Result<LoginResult> login =
            await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.WeakPassword);

        Result<LoginResult> result =
            await h.CreatePendingLoginService().ConfirmEmailByCodeAsync(login.Value.Token, h.LastCode());

        Assert.Equal(AuthenticationApiErrors.InvalidPendingToken, result.Error);
    }

    [Fact]
    public async Task Alternative_implementations_keep_the_steps_but_issue_no_tokens()
    {
        using var h = new LoginServicesHarness(emailConfirmed: false);
        var issuer = new CookieLikeTokenIssuer();
        var store = new CookieLikePendingStore();
        h.TokenIssuer = issuer;
        h.PendingStore = store;

        Result<LoginResult> login = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.Equal(ELoginNextStep.ConfirmEmail, login.Value.NextStep);
        Assert.Equal(string.Empty, login.Value.Token);
        Assert.Equal(new PendingTokenData(h.User.Id, ELoginNextStep.ConfirmEmail, false, true), store.Saved);
        Assert.Single(h.Sender.Sent);

        //sso-ში მდგომარეობა cookie-დან იკითხება, ამიტომ კლიენტი ტოკენს არ აგზავნის
        Result<LoginResult> confirmed = await h.CreatePendingLoginService().ConfirmEmailByCodeAsync(null, h.LastCode());

        Assert.Equal(ELoginNextStep.None, confirmed.Value.NextStep);
        Assert.Equal(string.Empty, confirmed.Value.Token);
        Assert.Same(h.User, Assert.Single(issuer.SignedIn));
        Assert.Equal(0, h.RefreshTokenRows);
    }

    [Fact]
    public async Task Alternative_issuer_receives_a_fully_checked_login()
    {
        using var h = new LoginServicesHarness();
        var issuer = new CookieLikeTokenIssuer();
        h.TokenIssuer = issuer;

        Result<LoginResult> login = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.Equal(ELoginNextStep.None, login.Value.NextStep);
        Assert.Same(h.User, Assert.Single(issuer.SignedIn));
    }

    [Fact]
    public async Task Email_sign_in_is_off_by_default()
    {
        using var h = new LoginServicesHarness();

        Result<LoginResult> result =
            await h.CreateLoginService().TryToLogin("tester@example.com", LoginServicesHarness.Password);

        Assert.Equal(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect, result.Error);
        h.UserMgr.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("tester@example.com")]
    [InlineData("tester")]
    public async Task With_email_sign_in_the_user_is_found_by_e_mail_or_by_name(string input)
    {
        using var h = new LoginServicesHarness(true);

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin(input, LoginServicesHarness.Password);

        Assert.True(result.IsSuccess);
        Assert.Same(h.User, result.Value.User);
    }

    [Fact]
    public async Task With_email_sign_in_an_input_with_at_is_looked_up_by_e_mail_first()
    {
        using var h = new LoginServicesHarness(true);

        await h.CreateLoginService().TryToLogin("tester@example.com", LoginServicesHarness.Password);

        h.UserMgr.Verify(m => m.FindByEmailAsync("tester@example.com"), Times.Once);
        h.UserMgr.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task With_email_sign_in_a_user_name_with_at_is_found_after_the_e_mail_miss()
    {
        using var h = new LoginServicesHarness(true, userName: "old@name", email: "other@example.com");

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("old@name", LoginServicesHarness.Password);

        Assert.True(result.IsSuccess);
        h.UserMgr.Verify(m => m.FindByEmailAsync("old@name"), Times.Once);
    }

    [Fact]
    public async Task With_email_sign_in_an_unknown_input_is_refused()
    {
        using var h = new LoginServicesHarness(true);

        Result<LoginResult> result =
            await h.CreateLoginService().TryToLogin("nobody@example.com", LoginServicesHarness.Password);

        Assert.Equal(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect, result.Error);
    }

    [Fact]
    public async Task A_refusing_check_stops_the_login_with_its_own_error_and_nothing_is_issued()
    {
        using var h = new LoginServicesHarness();
        h.Checks.Add(Refusing().Object);

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.Equal(Deactivated, result.Error);
        Assert.Equal(0, h.RefreshTokenRows);
    }

    [Fact]
    public async Task A_refusing_check_runs_before_a_code_is_mailed()
    {
        using var h = new LoginServicesHarness(emailConfirmed: false);
        h.Checks.Add(Refusing().Object);

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.Equal(Deactivated, result.Error);
        Assert.Empty(h.Sender.Sent);
    }

    [Fact]
    public async Task A_check_is_not_asked_when_the_password_is_wrong()
    {
        using var h = new LoginServicesHarness();
        Mock<ILoginCheck> check = Refusing();
        h.Checks.Add(check.Object);

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("tester", "wrong password 9");

        Assert.Equal(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect, result.Error);
        check.Verify(c => c.CheckAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task A_check_also_guards_the_full_login_after_a_pending_step()
    {
        using var h = new LoginServicesHarness(emailConfirmed: false);
        Result<LoginResult> login = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);
        h.Checks.Add(Refusing().Object);

        Result<LoginResult> confirmed = await h.CreatePendingLoginService()
            .ConfirmEmailByCodeAsync(login.Value.Token, h.LastCode());

        Assert.Equal(Deactivated, confirmed.Error);
        Assert.Equal(0, h.RefreshTokenRows);
    }

    [Fact]
    public async Task Passing_checks_let_the_login_through()
    {
        using var h = new LoginServicesHarness();
        var check = new Mock<ILoginCheck>();
        check.Setup(c => c.CheckAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        h.Checks.Add(check.Object);
        h.Checks.Add(check.Object);

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.Equal(ELoginNextStep.None, result.Value.NextStep);
        check.Verify(c => c.CheckAsync(h.User, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task A_locked_out_user_is_refused()
    {
        using var h = new LoginServicesHarness();
        h.LockedOut = true;

        Result<LoginResult> result = await h.CreateLoginService().TryToLogin("tester", LoginServicesHarness.Password);

        Assert.Equal(AuthenticationApiErrors.UserIsLockedOut, result.Error);
    }

    private static Mock<ILoginCheck> Refusing()
    {
        var check = new Mock<ILoginCheck>();
        check.Setup(c => c.CheckAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Deactivated));
        return check;
    }
}
