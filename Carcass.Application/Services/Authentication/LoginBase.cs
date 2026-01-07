using BackendCarcassContracts.Errors;
using Carcass.Application.Services.Authentication.Models;
using CarcassIdentity.Models;
using CarcassMasterDataDom.Models;
using CarcassRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.Services.Authentication;

public class LoginBase
{
    protected static int LastSequentialNumber;
    protected readonly UserManager<AppUser> UserMgr;
    private readonly IOptions<IdentitySettings> _identitySettings;
    private readonly SignInManager<AppUser> _signinMgr;
    private readonly IUserClaimsRepository? _userClaimsRepository;

    public LoginBase(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings, IUserClaimsRepository? userClaimsRepository = null)
    {
        _signinMgr = signinMgr;
        UserMgr = userMgr;
        _identitySettings = identitySettings;
        _userClaimsRepository = userClaimsRepository;
    }

    //ამ მეთოდით ავტორიზაციის პროცესი გამოტანილია ცალკე
    //და გამოიყენება როგორც ავტორიზაციისას, ისე ახალი მომხმარებლის დარეგისტრირებისას,
    //რომ ავტომატურად მოხდეს რეგისტრაციისას ავტორიზაციაც
    //public static async ValueTask<AppUser?> DoLogin(SignInManager<AppUser> signinMgr, AppUser? user, string password)
    //{
    //    if (user == null)
    //        return null;
    //    await signinMgr.SignOutAsync();
    //    var result = await signinMgr.PasswordSignInAsync(user, password, true, false);
    //    return result.Succeeded ? user : null;
    //}

    public async Task<OneOf<LoginResult, Err[]>> LoginProcess(AppUser? user, string password,
        CancellationToken cancellationToken = default)
    {
        if (user == null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        await _signinMgr.SignOutAsync();
        var result = await _signinMgr.PasswordSignInAsync(user, password, true, false);
        if (!result.Succeeded)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        if (user.UserName == null)
            return new[] { AuthenticationApiErrors.InvalidUsername };

        if (user.Email == null)
            return new[] { AuthenticationApiErrors.InvalidEmail };

        var roles = await UserMgr.GetRolesAsync(user);

        //ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
        //თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
        //IList<string> roles = await _userMgr.GetRolesAsync(user);

        if (_identitySettings.Value.JwtSecret is null)
            return new[] { CarcassApiErrors.ParametersAreInvalid };

        LastSequentialNumber++;
        var token = user.CreateJwToken(_identitySettings.Value.JwtSecret, LastSequentialNumber, roles);
        if (token is null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        var appClaims = _userClaimsRepository is null
            ? null
            : await _userClaimsRepository.UserAppClaims(user.UserName, cancellationToken);
        //var appUserModel = new LoginResponse(user.Id, _lastSequentialNumber, user.UserName, user.Email, token,
        //    roles.Aggregate(string.Empty, (cur, next) => cur + (cur == string.Empty ? string.Empty : ", ") + next),
        //    user.FirstName, user.LastName, await _userClaimsRepository.UserAppClaims(user.UserName, cancellationToken));
        //return appUserModel;

        return new LoginResult
        {
            User = user,
            LastSequentialNumber = LastSequentialNumber,
            Token = token,
            AppClaims = appClaims ?? [],
            Roles = roles
        };
    }
}