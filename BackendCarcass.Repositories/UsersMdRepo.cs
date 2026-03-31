using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Database.Models;
using BackendCarcass.Db;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public sealed class UsersMdRepo : IdentityCrudBase, IMdCrudRepo
{
    private readonly UserManager<AppUser> _userManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UsersMdRepo(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public OneOf<IQueryable<IDataType>, Error[]> Load()
    {
        return OneOf<IQueryable<IDataType>, Error[]>.FromT0(_userManager.Users.Cast<IDataType>());
    }

    public async Task<Option<Error[]>> Create(IDataType newItem)
    {
        var user = (User)newItem;
        var appUser = new AppUser(user.UserName, user.FirstName, user.LastName) { Email = user.Email };
        //შევქმნათ მომხმარებელი
        IdentityResult result = await _userManager.CreateAsync(appUser);
        user.UsrId = appUser.Id;
        return (Error[])ConvertError(result);
    }

    public async ValueTask<Option<Error[]>> Update(int id, IDataType newItem)
    {
        AppUser? oldUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldUser == null)
        {
            return new[] { MasterDataApiErrors.CannotFindUser };
        }

        var user = (User)newItem;
        oldUser.UserName = user.UserName;
        oldUser.Email = user.Email;
        oldUser.FirstName = user.FirstName;
        oldUser.LastName = user.LastName;

        IdentityResult updateResult = await _userManager.UpdateAsync(oldUser);
        if (!updateResult.Succeeded)
        {
            return (Error[])ConvertError(updateResult);
        }

        if (oldUser.UserName != user.UserName)
        {
            IdentityResult setUserNameResult = await _userManager.SetUserNameAsync(oldUser, user.UserName);
            if (!setUserNameResult.Succeeded)
            {
                return (Error[])ConvertError(setUserNameResult);
            }
        }

        if (oldUser.Email == user.Email)
        {
            return null;
        }

        IdentityResult setEmailResult = await _userManager.SetEmailAsync(oldUser, user.Email);
        return (Error[])ConvertError(setEmailResult);
    }

    public async ValueTask<Option<Error[]>> Delete(int id)
    {
        AppUser? oldUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldUser == null)
        {
            return new[] { MasterDataApiErrors.CannotFindUser };
        }

        IdentityResult deleteResult = await _userManager.DeleteAsync(oldUser);
        return (Error[])ConvertError(deleteResult);
    }
}
