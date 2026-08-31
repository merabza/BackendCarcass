using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Domain;
using BackendCarcass.Domain.Roles;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Repositories;

public sealed class RolesMdRepo : IdentityCrudBase, IMdCrudRepo
{
    private readonly RoleManager<AppRole> _roleManager;

    public RolesMdRepo(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public Result<IQueryable<IDataType>> Load()
    {
        return Result.Success(_roleManager.Roles.Cast<IDataType>());
    }

    public async Task<Result> Create(IDataType newItem)
    {
        var role = (Role)newItem;
        var appRole = new AppRole(role.RolKey, role.RolName, role.RolLevel);
        //შევქმნათ როლი
        IdentityResult result = await _roleManager.CreateAsync(appRole);
        role.RolId = appRole.Id;
        return ConvertError(result);
    }

    public async ValueTask<Result> Update(int id, IDataType newItem)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole == null)
        {
            return Result.Failure(MasterDataApiErrors.CannotFindRole);
        }

        var role = (Role)newItem;
        oldRole.RoleName = role.RolName;
        oldRole.Level = role.RolLevel;

        IdentityResult updateResult = await _roleManager.UpdateAsync(oldRole);
        if (!updateResult.Succeeded)
        {
            return ConvertError(updateResult);
        }

        if (oldRole.RoleName == role.RolKey)
        {
            return Result.Success();
        }

        IdentityResult setRoleResult = await _roleManager.SetRoleNameAsync(oldRole, role.RolKey);
        return ConvertError(setRoleResult);
    }

    public async ValueTask<Result> Delete(int id)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole == null)
        {
            return Result.Failure(MasterDataApiErrors.CannotFindRole);
        }

        IdentityResult deleteResult = await _roleManager.DeleteAsync(oldRole);
        return ConvertError(deleteResult);
    }
}
