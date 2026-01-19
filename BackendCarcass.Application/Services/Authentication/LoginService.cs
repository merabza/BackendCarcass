using System.Threading;
using System.Threading.Tasks;
using Carcass.Application.Services.Authentication.Models;
using CarcassIdentity.Models;
using CarcassMasterData.Models;
using CarcassRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.Services.Authentication;

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
    }
}