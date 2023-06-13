using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace BackendCarcassApi.Handlers.Authentication;

public /*open*/ class LoginCommandBase
{
    protected static int LastSequentialNumber;

    //ამ მეთოდით ავტორიზაციის პროცესი გამოტანილია ცალკე
    //და გამოიყენება როგორც ავტორიზაციისას, ისე ახალი მომხმარებლის დარეგისტრირებისას,
    //რომ ავტომატურად მოხდეს რეგისტრაციისას ავტორიზაციაც
    protected static async Task<AppUser?> DoLogin(SignInManager<AppUser> signinMgr, AppUser? user, string password)
    {
        if (user == null)
            return null;
        await signinMgr.SignOutAsync();
        var result = await signinMgr.PasswordSignInAsync(user, password, true, false);
        return result.Succeeded ? user : null;
    }
}