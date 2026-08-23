using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using BackendCarcassDomain.Entities;
using BackendCarcassDomain.Entities.Users;
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

    public OneOf<IQueryable<IDataType>, ErrorOmd[]> Load()
    {
        return OneOf<IQueryable<IDataType>, ErrorOmd[]>.FromT0(_userManager.Users.Cast<IDataType>());
    }

    public async Task<Option<ErrorOmd[]>> Create(IDataType newItem)
    {
        var user = (User)newItem;
        var appUser = new AppUser(user.UserName, user.FirstName, user.LastName) { Email = user.Email };
        //შევქმნათ მომხმარებელი
        IdentityResult result = await _userManager.CreateAsync(appUser);
        user.UsrId = appUser.Id;
        return (ErrorOmd[])ConvertError(result);
    }

    public async ValueTask<Option<ErrorOmd[]>> Update(int id, IDataType newItem)
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
            return (ErrorOmd[])ConvertError(updateResult);
        }

        if (oldUser.UserName != user.UserName)
        {
            IdentityResult setUserNameResult = await _userManager.SetUserNameAsync(oldUser, user.UserName);
            if (!setUserNameResult.Succeeded)
            {
                return (ErrorOmd[])ConvertError(setUserNameResult);
            }
        }

        if (oldUser.Email == user.Email)
        {
            return null;
        }

        IdentityResult setEmailResult = await _userManager.SetEmailAsync(oldUser, user.Email);
        return (ErrorOmd[])ConvertError(setEmailResult);
    }

    public async ValueTask<Option<ErrorOmd[]>> Delete(int id)
    {
        AppUser? oldUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldUser == null)
        {
            return new[] { MasterDataApiErrors.CannotFindUser };
        }

        IdentityResult deleteResult = await _userManager.DeleteAsync(oldUser);
        return (ErrorOmd[])ConvertError(deleteResult);
    }
}
