using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.MasterData.Models;
using BackendCarcassDomain.Entities;
using BackendCarcassDomain.Entities.Roles;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Repositories;

public sealed class RolesMdRepo : IdentityCrudBase, IMdCrudRepo
{
    private readonly RoleManager<AppRole> _roleManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RolesMdRepo(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public Result<IQueryable<IDataType>> Load()
    {
        return Result.Success(_roleManager.Roles.Cast<IDataType>());
    }

    public async Task<Option<ErrorOmd[]>> Create(IDataType newItem)
    {
        var role = (Role)newItem;
        var appRole = new AppRole(role.RolKey, role.RolName, role.RolLevel);
        //შევქმნათ როლი
        IdentityResult result = await _roleManager.CreateAsync(appRole);
        role.RolId = appRole.Id;
        return (ErrorOmd[])ConvertError(result);
    }

    public async ValueTask<Option<ErrorOmd[]>> Update(int id, IDataType newItem)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole == null)
        {
            return new[] { MasterDataApiErrors.CannotFindRole };
        }

        var role = (Role)newItem;
        oldRole.RoleName = role.RolName;
        oldRole.Level = role.RolLevel;

        IdentityResult updateResult = await _roleManager.UpdateAsync(oldRole);
        if (!updateResult.Succeeded)
        {
            return (ErrorOmd[])ConvertError(updateResult);
        }

        if (oldRole.RoleName == role.RolKey)
        {
            return null;
        }

        IdentityResult setRoleResult = await _roleManager.SetRoleNameAsync(oldRole, role.RolKey);
        return (ErrorOmd[])ConvertError(setRoleResult);
    }

    public async ValueTask<Option<ErrorOmd[]>> Delete(int id)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole == null)
        {
            return new[] { MasterDataApiErrors.CannotFindRole };
        }

        IdentityResult deleteResult = await _roleManager.DeleteAsync(oldRole);
        return (ErrorOmd[])ConvertError(deleteResult);
    }
}
