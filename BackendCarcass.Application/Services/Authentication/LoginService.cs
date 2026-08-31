using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Application.Services.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;

//using AppUser = BackendCarcass.Application.MasterData.Models.AppUser;
//using IdentitySettings = BackendCarcass.Application.Identity.Models.IdentitySettings;
//using IUserClaimsRepository = BackendCarcass.Application.Repositories.IUserClaimsRepository;

namespace BackendCarcass.Application.Services.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public class LoginService : LoginBase, IScopeServiceCarcassApplication
{
    public LoginService(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        IOptions<IdentitySettings> identitySettings, IUserClaimsRepository userClaimsRepository) : base(userMgr,
        signinMgr, identitySettings, userClaimsRepository)
    {
    }

    public async Task<Result<LoginResult>> TryToLogin(string userName, string password,
        CancellationToken cancellationToken = default)
    {
        //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
        AppUser? user = await UserMgr.FindByNameAsync(userName);

        return await LoginProcess(user, password, cancellationToken);
    }
}
