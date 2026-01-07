using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace Carcass.Application.Authentication;

public /*open*/ class LoginCommandHandlerBase
{
    protected static int LastSequentialNumber;

    //ამ მეთოდით ავტორიზაციის პროცესი გამოტანილია ცალკე
    //და გამოიყენება როგორც ავტორიზაციისას, ისე ახალი მომხმარებლის დარეგისტრირებისას,
    //რომ ავტომატურად მოხდეს რეგისტრაციისას ავტორიზაციაც
    protected static async ValueTask<AppUser?> DoLogin(SignInManager<AppUser> signinMgr, AppUser? user, string password)
    {
        if (user == null)
            return null;
        await signinMgr.SignOutAsync();
        var result = await signinMgr.PasswordSignInAsync(user, password, true, false);
        return result.Succeeded ? user : null;
    }
}