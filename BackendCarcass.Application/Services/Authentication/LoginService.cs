using System;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Services.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public class LoginService : LoginBase, IScopeServiceCarcassApplication
{
    private readonly IdentitySettings _identitySettings;

    public LoginService(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr,
        LoginCompletionService loginCompletion, PasswordService passwordService,
        IOptions<IdentitySettings> identitySettings) : base(userMgr, signinMgr, loginCompletion, passwordService)
    {
        _identitySettings = identitySettings.Value;
    }

    //userName შეიძლება ელფოსტაც იყოს, თუ IdentitySettings:AllowEmailSignIn ჩართულია
    public async Task<Result<LoginResult>> TryToLogin(string userName, string password,
        CancellationToken cancellationToken = default)
    {
        AppUser? user = await FindUserAsync(userName);

        return await LoginProcess(user, password, cancellationToken);
    }

    //@-იანი შეყვანა ჯერ ელფოსტით იძებნება, სხვა — სახელით; ვერ პოვნისას მეორე გზაც იცდება
    private async Task<AppUser?> FindUserAsync(string userNameOrEmail)
    {
        if (!_identitySettings.AllowEmailSignIn)
        {
            return await UserMgr.FindByNameAsync(userNameOrEmail);
        }

        if (userNameOrEmail.Contains('@', StringComparison.Ordinal))
        {
            return await UserMgr.FindByEmailAsync(userNameOrEmail) ?? await UserMgr.FindByNameAsync(userNameOrEmail);
        }

        return await UserMgr.FindByNameAsync(userNameOrEmail) ?? await UserMgr.FindByEmailAsync(userNameOrEmail);
    }
}
