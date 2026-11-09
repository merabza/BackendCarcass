using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Email;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Domain.Users;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Tests.TestInfrastructure;

// The sign-in services wired as in the host: the real e-mail verification, refresh tokens and pending tokens on
// in-memory SQLite, UserManager and SignInManager mocked. The password check succeeds for Password and WeakPassword;
// WeakPassword breaks the policy, so it leads to the forced password change
public sealed class LoginServicesHarness : IDisposable
{
    public const string Password = "correct horse 1";
    public const string WeakPassword = "weak1";
    public const string NewPassword = "new strong password 2";
    public const string Secret = "0123456789abcdef0123456789abcdef0123456789abcdef";

    private readonly CarcassSqliteDatabase _db = new();

    public LoginServicesHarness(bool allowEmailSignIn = false, bool emailConfirmed = true,
        string userName = "tester", string email = "tester@example.com")
    {
        Settings = new IdentitySettings
        {
            JwtSecret = Secret,
            JwtIssuer = "https://issuer.test",
            JwtAudience = "https://audience.test",
            FrontendBaseUrl = "https://app.test",
            AllowEmailSignIn = allowEmailSignIn
        };

        User row = _db.AddUser(userName, email);
        User = new AppUser(row.UserName, row.FirstName, row.LastName)
        {
            Id = row.UsrId, Email = row.Email, EmailConfirmed = emailConfirmed
        };

        var policy = new Mock<IPasswordValidator<AppUser>>();
        policy.Setup(v => v.ValidateAsync(It.IsAny<UserManager<AppUser>>(), It.IsAny<AppUser>(), It.IsAny<string?>()))
            .ReturnsAsync((UserManager<AppUser> _, AppUser _, string? password) => password == WeakPassword
                ? IdentityResult.Failed(new IdentityError { Code = "Weak", Description = "სუსტი პაროლი" })
                : IdentityResult.Success);

        UserMgr = new Mock<UserManager<AppUser>>(Mock.Of<IUserStore<AppUser>>(), null!, new PasswordHasher<AppUser>(), null!,
            new[] { policy.Object }, null!, null!, null!, null!);
        UserMgr.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => name == User.UserName ? User : null);
        UserMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string address) => address == User.Email ? User : null);
        UserMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => id == User.Id.ToString(CultureInfo.InvariantCulture) ? User : null);
        UserMgr.Setup(m => m.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(["Editor"]);
        UserMgr.Setup(m => m.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);

        SignInMgr = new Mock<SignInManager<AppUser>>(UserMgr.Object, Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(), null!, null!, null!, null!);
        SignInMgr.Setup(m => m.CheckPasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<string>(), true))
            .ReturnsAsync((AppUser _, string password, bool _) => CheckPassword(password));

        Tokens = new AuthTokenService(Options.Create(Settings));
        RefreshTokens = new RefreshTokenService(_db.Context, Options.Create(Settings));
        EmailVerification = new EmailVerificationService(_db.Context, Sender, Options.Create(Settings),
            Options.Create(new EmailSettings()), NullLogger<EmailVerificationService>.Instance);

        var claims = new Mock<IUserClaimsRepository>();
        claims.Setup(r => r.UserAppClaims(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(["ManageUsers"]);
        TokenIssuer = new JwtLoginTokenIssuer(UserMgr.Object, Tokens, RefreshTokens, claims.Object);
        PendingStore = new JwtPendingLoginStore(Tokens);
    }

    public IdentitySettings Settings { get; }
    public AppUser User { get; }
    public Mock<UserManager<AppUser>> UserMgr { get; }
    public Mock<SignInManager<AppUser>> SignInMgr { get; }
    public AuthTokenService Tokens { get; }
    public RefreshTokenService RefreshTokens { get; }
    public EmailVerificationService EmailVerification { get; }
    public CapturingEmailSender Sender { get; } = new();
    public List<ILoginCheck> Checks { get; } = [];
    public bool LockedOut { get; set; }

    //ნაგულისხმევად JWT-ის რეალიზაციებია; ტესტს შეუძლია სხვით ჩაანაცვლოს
    public ILoginTokenIssuer TokenIssuer { get; set; }
    public IPendingLoginStore PendingStore { get; set; }

    public int RefreshTokenRows => _db.Context.RefreshTokens.Count();

    public void Dispose()
    {
        _db.Dispose();
    }

    public LoginCompletionService CreateCompletion()
    {
        return new LoginCompletionService(TokenIssuer, PendingStore, EmailVerification, Checks);
    }

    public LoginService CreateLoginService()
    {
        return new LoginService(UserMgr.Object, SignInMgr.Object, CreateCompletion(), CreatePasswordService(),
            Options.Create(Settings));
    }

    public PendingLoginService CreatePendingLoginService()
    {
        return new PendingLoginService(UserMgr.Object, PendingStore, EmailVerification, CreateCompletion(),
            CreatePasswordService());
    }

    //ბოლოს გაგზავნილი წერილის 6-ნიშნა კოდი
    public string LastCode()
    {
        return Regex.Match(Sender.Last.Body, @"^\d{6}\r?$", RegexOptions.Multiline, TimeSpan.FromSeconds(1)).Value
            .TrimEnd('\r');
    }

    private SignInResult CheckPassword(string password)
    {
        if (LockedOut)
        {
            return SignInResult.LockedOut;
        }

        return password is Password or WeakPassword ? SignInResult.Success : SignInResult.Failed;
    }

    private PasswordService CreatePasswordService()
    {
        return new PasswordService(UserMgr.Object, RefreshTokens);
    }
}

// sso-ს მსგავსი რეალიზაცია: სრული წვდომისას ტოკენი არ იქმნება (სესია cookie-შია)
public sealed class CookieLikeTokenIssuer : ILoginTokenIssuer
{
    public List<AppUser> SignedIn { get; } = [];

    public Task<Result<LoginResult>>
        IssueFullAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        SignedIn.Add(user);
        return Task.FromResult(Result.Success(
            new LoginResult
            {
                User = user, Token = string.Empty, AppClaims = [], Roles = []
            }));
    }
}

// sso-ს მსგავსი რეალიზაცია: მდგომარეობა სერვერზეა (cookie-ის იმიტაცია), კლიენტი ტოკენს არ იღებს და არ აბრუნებს
public sealed class CookieLikePendingStore : IPendingLoginStore
{
    public PendingTokenData? Saved { get; private set; }

    public Result<string> Save(AppUser user,
        PendingTokenData data)
    {
        Saved = data;
        return Result.Success(string.Empty);
    }

    public Result<PendingTokenData> Read(
        string? token)
    {
        return Saved is null
            ? Result.Failure<PendingTokenData>(
                AuthenticationApiErrors.InvalidPendingToken)
            : Result.Success(Saved);
    }
}
