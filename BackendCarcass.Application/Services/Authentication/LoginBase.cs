using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

public class LoginBase
{
    protected readonly UserManager<AppUser> UserMgr;
    private readonly LoginCompletionService _loginCompletion;
    private readonly PasswordService _passwordService;
    private readonly SignInManager<AppUser> _signinMgr;

    public LoginBase(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        LoginCompletionService loginCompletion, PasswordService passwordService)
    {
        UserMgr = userMgr;
        _signinMgr = signinMgr;
        _loginCompletion = loginCompletion;
        _passwordService = passwordService;
    }

    public async Task<Result<LoginResult>> LoginProcess(AppUser? user, string password,
        CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect);
        }

        //lockoutOnFailure: true -> წარუმატებელი ცდები ითვლება და ზღვრის მიღწევისას ანგარიში დროებით იბლოკება
        SignInResult result = await _signinMgr.CheckPasswordSignInAsync(user, password, true);
        if (result.IsLockedOut)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.UserIsLockedOut);
        }

        if (!result.Succeeded)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.UsernameOrPasswordIsIncorrect);
        }

        if (user.UserName == null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.InvalidUsername);
        }

        if (user.Email == null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.InvalidEmail);
        }

        //პაროლი სწორია, მაგრამ შეიძლება მოქმედ წესებს ვეღარ აკმაყოფილებდეს (წესები მას შემდეგ გამკაცრდა)
        bool passwordChangeRequired = !await _passwordService.MeetsPolicyAsync(user, password);
        return await _loginCompletion.CompleteAsync(user, passwordChangeRequired, cancellationToken);
    }
}
