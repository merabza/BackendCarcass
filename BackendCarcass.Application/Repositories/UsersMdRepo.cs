using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Domain;
using BackendCarcass.Domain.Users;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Repositories;

public sealed class UsersMdRepo : IdentityCrudBase, IMdCrudRepo
{
    private readonly UserManager<AppUser> _userManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UsersMdRepo(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public Result<IQueryable<IDataType>> Load()
    {
        return Result.Success(_userManager.Users.Cast<IDataType>());
    }

    public async Task<Result> Create(IDataType newItem)
    {
        var user = (User)newItem;
        var appUser = new AppUser(user.UserName, user.FirstName, user.LastName) { Email = user.Email };
        //შევქმნათ მომხმარებელი
        IdentityResult result = await _userManager.CreateAsync(appUser);
        user.UsrId = appUser.Id;
        return ConvertError(result);
    }

    public async ValueTask<Result> Update(int id, IDataType newItem)
    {
        AppUser? oldUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldUser == null)
        {
            return Result.Failure(MasterDataApiErrors.CannotFindUser.ToError());
        }

        var user = (User)newItem;
        oldUser.UserName = user.UserName;
        oldUser.Email = user.Email;
        oldUser.FirstName = user.FirstName;
        oldUser.LastName = user.LastName;

        IdentityResult updateResult = await _userManager.UpdateAsync(oldUser);
        if (!updateResult.Succeeded)
        {
            return ConvertError(updateResult);
        }

        if (oldUser.UserName != user.UserName)
        {
            IdentityResult setUserNameResult = await _userManager.SetUserNameAsync(oldUser, user.UserName);
            if (!setUserNameResult.Succeeded)
            {
                return ConvertError(setUserNameResult);
            }
        }

        if (oldUser.Email == user.Email)
        {
            return Result.Success();
        }

        IdentityResult setEmailResult = await _userManager.SetEmailAsync(oldUser, user.Email);
        return ConvertError(setEmailResult);
    }

    public async ValueTask<Result> Delete(int id)
    {
        AppUser? oldUser = await _userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldUser == null)
        {
            return Result.Failure(MasterDataApiErrors.CannotFindUser.ToError());
        }

        IdentityResult deleteResult = await _userManager.DeleteAsync(oldUser);
        return ConvertError(deleteResult);
    }
}
