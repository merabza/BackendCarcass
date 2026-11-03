using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public class RegistrationService : LoginBase, IScopeServiceCarcassApplication
{
    public RegistrationService(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        LoginCompletionService loginCompletion, PasswordService passwordService) : base(userMgr, signinMgr,
        loginCompletion, passwordService)
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

        //მოწოდებული ელფოსტის მისამართით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        user = await UserMgr.FindByEmailAsync(registerParameters.Email);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null)
        {
            return Result.Failure<LoginResult>(AuthenticationApiErrors.EmailAlreadyExists);
        }

        //1. შევქმნათ ახალი მომხმარებელი. ელფოსტა ჯერ დაუდასტურებელია
        user = new AppUser(registerParameters.UserName, registerParameters.FirstName, registerParameters.LastName)
        {
            Email = registerParameters.Email, EmailConfirmed = false
        };
        IdentityResult result = await UserMgr.CreateAsync(user, registerParameters.Password);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა (მაგალითად პაროლი წესებს არ აკმაყოფილებს), ვჩერდებით
        if (!result.Succeeded)
        {
            return Result.Failure<LoginResult>(result.ToError(AuthenticationApiErrors.CouldNotCreateNewUser));
        }

        //2. ავტორიზაციის პროცესი: ელფოსტა დაუდასტურებელია, ამიტომ მომხმარებელი მიიღებს დროებით ტოკენს
        //   და ელფოსტაზე გაეგზავნება დადასტურების კოდი და ბმული
        return await LoginProcess(user, registerParameters.Password, cancellationToken);
    }
}
