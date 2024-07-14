using CarcassDb;
using CarcassDb.Models;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using OneOf;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using SystemToolsShared.Errors;

namespace CarcassRepositories;

public sealed class RolesMdRepo : IdentityCrudBase, IMdCrudRepo
{
    private readonly RoleManager<AppRole> _roleManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RolesMdRepo(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public OneOf<IQueryable<IDataType>, Err[]> Load()
    {
        return OneOf<IQueryable<IDataType>, Err[]>.FromT0(_roleManager.Roles.Cast<IDataType>());
    }

    public async Task<Option<Err[]>> Create(IDataType newItem)
    {
        var role = (Role)newItem;
        AppRole appRole = new(role.RolKey, role.RolName, role.RolLevel);
        //შევქმნათ როლი
        var result = await _roleManager.CreateAsync(appRole);
        role.RolId = appRole.Id;
        return ConvertError(result);
    }

    public async Task<Option<Err[]>> Update(int id, IDataType newItem)
    {
        var oldRole = await _roleManager.FindByIdAsync(id.ToString());
        if (oldRole == null)
            return new[] { MasterDataApiErrors.CannotFindRole };

        var role = (Role)newItem;
        oldRole.RoleName = role.RolName;
        oldRole.Level = role.RolLevel;

        var updateResult = await _roleManager.UpdateAsync(oldRole);
        if (!updateResult.Succeeded)
            return ConvertError(updateResult);

        if (oldRole.RoleName == role.RolKey)
            return null;

        var setRoleResult = await _roleManager.SetRoleNameAsync(oldRole, role.RolKey);
        return ConvertError(setRoleResult);
    }

    public async Task<Option<Err[]>> Delete(int id)
    {
        var oldRole = await _roleManager.FindByIdAsync(id.ToString());
        if (oldRole == null)
            return new[] { MasterDataApiErrors.CannotFindRole };
        var deleteResult = await _roleManager.DeleteAsync(oldRole);
        return ConvertError(deleteResult);
    }
}