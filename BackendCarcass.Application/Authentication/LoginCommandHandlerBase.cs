using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using AppUser = BackendCarcass.Application.MasterData.Models.AppUser;

namespace BackendCarcass.Application.Authentication;

public /*open*/ class LoginCommandHandlerBase
{
    //ამ მეთოდით ავტორიზაციის პროცესი გამოტანილია ცალკე
    //და გამოიყენება როგორც ავტორიზაციისას, ისე ახალი მომხმარებლის დარეგისტრირებისას,
    //რომ ავტომატურად მოხდეს რეგისტრაციისას ავტორიზაციაც
    protected static async ValueTask<AppUser?> DoLogin(SignInManager<AppUser> signinMgr, AppUser? user, string password)
    {
        if (user == null)
        {
            return null;
        }

        await signinMgr.SignOutAsync();
        SignInResult result = await signinMgr.PasswordSignInAsync(user, password, true, false);
        return result.Succeeded ? user : null;
    }
}
