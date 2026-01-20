using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Identity.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcassContracts.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Services.Authentication;

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
        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        var user = await UserMgr.FindByNameAsync(registerParameters.UserName);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null) return new[] { AuthenticationApiErrors.UserAlreadyExists };

        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        user = await UserMgr.FindByEmailAsync(registerParameters.Email);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null) return new[] { AuthenticationApiErrors.EmailAlreadyExists };

        //1. შევქმნათ ახალი მომხმარებელი
        user = new AppUser(registerParameters.UserName, registerParameters.FirstName, registerParameters.LastName)
        {
            Email = registerParameters.Email
        };
        var result = await UserMgr.CreateAsync(user, registerParameters.Password);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (!result.Succeeded) return new[] { AuthenticationApiErrors.MoreComplexPasswordIsRequired };

        return await LoginProcess(user, registerParameters.Password, cancellationToken);
    }
}