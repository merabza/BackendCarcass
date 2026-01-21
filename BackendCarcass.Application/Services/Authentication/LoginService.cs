using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.Identity.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcass.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Services.Authentication;

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
        AppUser? user = await UserMgr.FindByNameAsync(userName);

        return await LoginProcess(user, password, cancellationToken);
    }
}
