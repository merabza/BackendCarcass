using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Authentication;
using BackendCarcassContracts.Errors;
using BackendCarcassContracts.V1.Responses;
using CarcassIdentity.Models;
using CarcassMasterDataDom.Models;
using CarcassRepositories;
using MessagingAbstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LoginCommandHandler : LoginCommandBase, ICommandHandler<LoginCommandRequest, LoginResponse>
{
    private readonly IOptions<IdentitySettings> _identitySettings;
    private readonly IMenuRightsRepository _mdRepo;
    private readonly SignInManager<AppUser> _signinMgr;
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public LoginCommandHandler(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings, IMenuRightsRepository mdRepo)
    {
        _userMgr = userMgr;
        _signinMgr = signinMgr;
        _identitySettings = identitySettings;
        _mdRepo = mdRepo;
    }

    public async Task<OneOf<LoginResponse, IEnumerable<Err>>> Handle(LoginCommandRequest request,
        CancellationToken cancellationToken)
    {
        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        var user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        //აქ მოვალთ მხოლოდ იმ შემთხვევაში, თუ მომხმარებელი წარმატებით შეიქმნა,
        //მოხდეს მისი ავტომატური ავტორიზაცია
        user = await DoLogin(_signinMgr, user, request.Password!);
        if (user == null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        if (user.UserName == null)
            return new[] { AuthenticationApiErrors.InvalidUsername };

        if (user.Email == null)
            return new[] { AuthenticationApiErrors.InvalidEmail };

        var roles = await _userMgr.GetRolesAsync(user);

        //ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
        //თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
        //IList<string> roles = await _userMgr.GetRolesAsync(user);

        if (_identitySettings.Value.JwtSecret is null)
            return new[] { CarcassApiErrors.ParametersAreInvalid };

        LastSequentialNumber++;
        var token = user.CreateJwToken(_identitySettings.Value.JwtSecret, LastSequentialNumber, roles);
        if (token is null)
            return new[] { AuthenticationApiErrors.UsernameOrPasswordIsIncorrect };

        LoginResponse appUserModel = new(user.Id, LastSequentialNumber, user.UserName, user.Email, token,
            roles.Aggregate(string.Empty, (cur, next) => cur + (cur == string.Empty ? string.Empty : ", ") + next),
            user.FirstName, user.LastName, await _mdRepo.UserAppClaims(user.UserName, cancellationToken));
        return appUserModel;
    }
}