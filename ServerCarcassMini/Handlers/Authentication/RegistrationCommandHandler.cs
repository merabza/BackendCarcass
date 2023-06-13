using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassContracts.V1.Responses;
using CarcassIdentity.Models;
using CarcassMasterDataDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using ServerCarcassMini.CommandRequests.Authentication;
using SystemToolsShared;

namespace ServerCarcassMini.Handlers.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RegistrationCommandHandler : LoginCommandBase,
    ICommandHandler<RegistrationCommandRequest, LoginResponse>
{
    private readonly IOptions<IdentitySettings> _identitySettings;
    private readonly SignInManager<AppUser> _signinMgr;
    private readonly UserManager<AppUser> _userMgr;

    public RegistrationCommandHandler(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings)
    {
        _userMgr = userMgr;
        _signinMgr = signinMgr;
        _identitySettings = identitySettings;
    }

    public async Task<OneOf<LoginResponse, IEnumerable<Err>>> Handle(RegistrationCommandRequest request,
        CancellationToken cancellationToken)
    {
        //if (string.IsNullOrWhiteSpace(request.UserName))
        //    return await Task.FromResult(new[]
        //        { CarcassApiErrors.IsEmpty(nameof(request.UserName), "მომხმარებლის სახელი") });

        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        var user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null)
            return new[] { AuthenticationApiErrors.UserAlreadyExists };


        //if (request.Email == null)
        //    return await Task.FromResult(new[]
        //        { CarcassApiErrors.IsEmpty(nameof(request.Email), "ელექტრონული ფოსტის მისამართი") });

        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        user = await _userMgr.FindByEmailAsync(request.Email!);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user != null)
            return new[] { AuthenticationApiErrors.EmailAlreadyExists };

        //1. შევქმნათ ახალი მომხმარებელი
        user = new AppUser(request.UserName!, request.FirstName!, request.LastName!) { Email = request.Email };
        var result = await _userMgr.CreateAsync(user, request.Password!);
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (!result.Succeeded)
            return new[] { AuthenticationApiErrors.MoreComplexPasswordIsRequired };

        //აქ მოვალთ მხოლოდ იმ შემთხვევაში, თუ მომხმარებელი წარმატებით შეიქმნა,
        //მოხდეს მისი ავტომატური ავტორიზაცია
        user = await DoLogin(_signinMgr, user, request.Password!);
        if (user == null)
            return new[] { AuthenticationApiErrors.CouldNotCreateNewUser };

        if (user.UserName == null)
            return new[] { AuthenticationApiErrors.InvalidUsername };

        if (user.Email == null)
            return new[] { AuthenticationApiErrors.InvalidEmail };

        //ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
        //თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
        //IList<string> roles = _userMgr.GetRolesAsync(user).Result;

        if (_identitySettings.Value.JwtSecret is null)
            return new[] { CarcassApiErrors.ParametersAreInvalid };

        var token = user.CreateJwToken(_identitySettings.Value.JwtSecret, LastSequentialNumber);

        if (token is null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };


        LastSequentialNumber++;
        LoginResponse appUserModel = new(user.Id, LastSequentialNumber, user.UserName, user.Email, token,
            user.FirstName, user.LastName, "");
        return appUserModel;
    }
}