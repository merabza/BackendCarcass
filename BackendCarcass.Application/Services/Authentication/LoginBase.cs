using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Identity.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcass.Repositories;
using BackendCarcassContracts.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Services.Authentication;

public class LoginBase
{
    private readonly IOptions<IdentitySettings> _identitySettings;
    private readonly SignInManager<AppUser> _signinMgr;
    private readonly IUserClaimsRepository? _userClaimsRepository;
    protected readonly UserManager<AppUser> UserMgr;

    public LoginBase(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings, IUserClaimsRepository? userClaimsRepository = null)
    {
        _signinMgr = signinMgr;
        UserMgr = userMgr;
        _identitySettings = identitySettings;
        _userClaimsRepository = userClaimsRepository;
    }

    public async Task<OneOf<LoginResult, Err[]>> LoginProcess(AppUser? user, string password,
        CancellationToken cancellationToken = default)
    {
        if (user == null) return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        await _signinMgr.SignOutAsync();
        var result = await _signinMgr.PasswordSignInAsync(user, password, true, false);
        if (!result.Succeeded) return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        if (user.UserName == null) return new[] { AuthenticationApiErrors.InvalidUsername };

        if (user.Email == null) return new[] { AuthenticationApiErrors.InvalidEmail };

        var roles = await UserMgr.GetRolesAsync(user);

        //ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
        //თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
        //IList<string> roles = await _userMgr.GetRolesAsync(user);

        if (_identitySettings.Value.JwtSecret is null) return new[] { CarcassApiErrors.ParametersAreInvalid };

        var token = user.CreateJwToken(_identitySettings.Value.JwtSecret, 0, roles);
        if (token is null) return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        var appClaims = _userClaimsRepository is null
            ? null
            : await _userClaimsRepository.UserAppClaims(user.UserName, cancellationToken);

        return new LoginResult
        {
            User = user,
            LastSequentialNumber = 0,
            Token = token,
            AppClaims = appClaims ?? [],
            Roles = roles
        };
    }
}