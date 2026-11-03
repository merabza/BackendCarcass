using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public class LoginService : LoginBase, IScopeServiceCarcassApplication
{
    public LoginService(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        LoginCompletionService loginCompletion, PasswordService passwordService) : base(userMgr, signinMgr,
        loginCompletion, passwordService)
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
