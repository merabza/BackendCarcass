using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public class RegistrationService : LoginBase, IScopeServiceCarcassApplication
{
    public RegistrationService(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings) : base(userMgr, signinMgr, identitySettings)
    {
    }

    public async Task<Result<LoginResult>> TryToRegister(RegisterParameters registerParameters,
        CancellationToken cancellationToken = default)
    {
        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        AppUser? user = await UserMgr.FindByNameAsync(registerParameters.UserName);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.UserAlreadyExists);
        }

        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        user = await UserMgr.FindByEmailAsync(registerParameters.Email);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.EmailAlreadyExists);
        }

        //1. შევქმნათ ახალი მომხმარებელი
        user = new AppUser(registerParameters.UserName, registerParameters.FirstName, registerParameters.LastName)
        {
            Email = registerParameters.Email
        };
        IdentityResult result = await UserMgr.CreateAsync(user, registerParameters.Password);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (!result.Succeeded)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.MoreComplexPasswordIsRequired);
        }

        return await LoginProcess(user, registerParameters.Password, cancellationToken);
    }
}
