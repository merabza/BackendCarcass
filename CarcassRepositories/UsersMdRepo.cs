using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassDb;
using CarcassDb.Models;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassRepositories;

public sealed class UsersMdRepo : IdentityCrudBase, IMdCrudRepo
{
    private readonly UserManager<AppUser> _userManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UsersMdRepo(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public OneOf<IQueryable<IDataType>, IEnumerable<Err>> Load()
    {
        return OneOf<IQueryable<IDataType>, IEnumerable<Err>>.FromT0(_userManager.Users.Cast<IDataType>()); 
    }

    public async Task<Option<IEnumerable<Err>>> Create(IDataType newItem)
    {
        var user = (User)newItem;
        var appUser = new AppUser(user.UserName, user.FirstName, user.LastName) { Email = user.Email };
        //შევქმნათ მომხმარებელი
        var result = await _userManager.CreateAsync(appUser);
        user.UsrId = appUser.Id;
        return (Err[])ConvertError(result);
    }

    public async ValueTask<Option<IEnumerable<Err>>> Update(int id, IDataType newItem)
    {
        var oldUser = await _userManager.FindByIdAsync(id.ToString());
        if (oldUser == null)
            return new[] { MasterDataApiErrors.CannotFindUser };
        var user = (User)newItem;
        oldUser.UserName = user.UserName;
        oldUser.Email = user.Email;
        oldUser.FirstName = user.FirstName;
        oldUser.LastName = user.LastName;

        var updateResult = await _userManager.UpdateAsync(oldUser);
        if (!updateResult.Succeeded)
            return (Err[])ConvertError(updateResult);

        if (oldUser.UserName != user.UserName)
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(oldUser, user.UserName);
            if (!setUserNameResult.Succeeded)
                return (Err[])ConvertError(setUserNameResult);
        }

        if (oldUser.Email == user.Email)
            return null;
        var setEmailResult = await _userManager.SetEmailAsync(oldUser, user.Email);
        return (Err[])ConvertError(setEmailResult);
    }

    public async ValueTask<Option<IEnumerable<Err>>> Delete(int id)
    {
        var oldUser = await _userManager.FindByIdAsync(id.ToString());
        if (oldUser == null)
            return new[] { MasterDataApiErrors.CannotFindUser };
        var deleteResult = await _userManager.DeleteAsync(oldUser);
        return (Err[])ConvertError(deleteResult);
    }
}