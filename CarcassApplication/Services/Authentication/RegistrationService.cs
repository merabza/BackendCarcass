using BackendCarcassContracts.Errors;
using CarcassApplication.Services.Authentication.Models;
using CarcassIdentity.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassApplication.Services.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public class RegistrationService : LoginBase, IScopeServiceCarcassApplication
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public RegistrationService(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings) : base(userMgr, signinMgr, identitySettings)
    {
    }

    public async Task<OneOf<LoginResult, Err[]>> TryToRegister(RegisterParameters registerParameters,
        CancellationToken cancellationToken = default)
    {
        //if (string.IsNullOrWhiteSpace(request.UserName))
        //    return await Task.FromResult(new[]
        //        { CarcassApiErrors.IsEmpty(nameof(request.UserName), "მომხმარებლის სახელი") });

        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        var user = await UserMgr.FindByNameAsync(registerParameters.UserName);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null)
            return new[] { AuthenticationApiErrors.UserAlreadyExists };

        //if (request.Email == null)
        //    return await Task.FromResult(new[]
        //        { CarcassApiErrors.IsEmpty(nameof(request.Email), "ელექტრონული ფოსტის მისამართი") });

        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        user = await UserMgr.FindByEmailAsync(registerParameters.Email);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null)
            return new[] { AuthenticationApiErrors.EmailAlreadyExists };

        //1. შევქმნათ ახალი მომხმარებელი
        user = new AppUser(registerParameters.UserName, registerParameters.FirstName, registerParameters.LastName)
        {
            Email = registerParameters.Email
        };
        var result = await UserMgr.CreateAsync(user, registerParameters.Password);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (!result.Succeeded)
            return new[] { AuthenticationApiErrors.MoreComplexPasswordIsRequired };

        return await LoginProcess(user, registerParameters.Password, cancellationToken);

        ////აქ მოვალთ მხოლოდ იმ შემთხვევაში, თუ მომხმარებელი წარმატებით შეიქმნა,
        ////მოხდეს მისი ავტომატური ავტორიზაცია
        //user = await DoLogin(_signinMgr, user, registerParameters.Password);
        //if (user == null)
        //    return new[] { AuthenticationApiErrors.CouldNotCreateNewUser };

        //if (user.UserName == null)
        //    return new[] { AuthenticationApiErrors.InvalidUsername };

        //if (user.Email == null)
        //    return new[] { AuthenticationApiErrors.InvalidEmail };

        ////ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
        ////თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
        ////IList<string> roles = _userMgr.GetRolesAsync(user).Result;

        //if (_identitySettings.Value.JwtSecret is null)
        //    return new[] { CarcassApiErrors.ParametersAreInvalid };

        //var token = user.CreateJwToken(_identitySettings.Value.JwtSecret, LastSequentialNumber);

        //if (token is null)
        //    return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        //LastSequentialNumber++;

        //return new LoginResult { User = user, LastSequentialNumber = LastSequentialNumber, Token = token };
    }
}