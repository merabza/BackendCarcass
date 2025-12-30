using CarcassApplication.Repositories;
using CarcassApplication.Services.Authentication.Models;
using CarcassIdentity.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassApplication.Services.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public class LoginService : LoginBase, IScopeServiceCarcassApplication
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public LoginService(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings, IUserClaimsRepository userClaimsRepository) : base(userMgr,
        signinMgr, identitySettings, userClaimsRepository)
    {
    }

    public async Task<OneOf<LoginResult, Err[]>> TryToLogin(string userName, string password,
        CancellationToken cancellationToken = default)
    {
        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        var user = await UserMgr.FindByNameAsync(userName);

        return await LoginProcess(user, password, cancellationToken);

        ////თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        //if (user == null)
        //    return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        ////აქ მოვალთ მხოლოდ იმ შემთხვევაში, თუ მომხმარებელი წარმატებით შეიქმნა,
        ////მოხდეს მისი ავტომატური ავტორიზაცია
        //user = await DoLogin(_signinMgr, user, password);
        //if (user == null)
        //    return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        //if (user.UserName == null)
        //    return new[] { AuthenticationApiErrors.InvalidUsername };

        //if (user.Email == null)
        //    return new[] { AuthenticationApiErrors.InvalidEmail };

        //var roles = await _userMgr.GetRolesAsync(user);

        ////ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
        ////თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
        ////IList<string> roles = await _userMgr.GetRolesAsync(user);

        //if (_identitySettings.Value.JwtSecret is null)
        //    return new[] { CarcassApiErrors.ParametersAreInvalid };

        //LastSequentialNumber++;
        //var token = user.CreateJwToken(_identitySettings.Value.JwtSecret, LastSequentialNumber, roles);
        //if (token is null)
        //    return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        //var appClaims = await _userClaimsRepository.UserAppClaims(user.UserName, cancellationToken);
        ////var appUserModel = new LoginResponse(user.Id, _lastSequentialNumber, user.UserName, user.Email, token,
        ////    roles.Aggregate(string.Empty, (cur, next) => cur + (cur == string.Empty ? string.Empty : ", ") + next),
        ////    user.FirstName, user.LastName, await _userClaimsRepository.UserAppClaims(user.UserName, cancellationToken));
        ////return appUserModel;

        //return new LoginResult
        //{
        //    User = user,
        //    LastSequentialNumber = LastSequentialNumber,
        //    Token = token,
        //    AppClaims = appClaims,
        //    Roles = roles
        //};
    }
}