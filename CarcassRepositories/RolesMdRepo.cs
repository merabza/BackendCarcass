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

public sealed class RolesMdRepo : IdentityCrudBase, IMdCrudRepo
{
    private readonly RoleManager<AppRole> _roleManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RolesMdRepo(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public OneOf<IQueryable<IDataType>, IEnumerable<Err>> Load()
    {
        return OneOf<IQueryable<IDataType>, IEnumerable<Err>>.FromT0(_roleManager.Roles.Cast<IDataType>());
    }

    public async Task<Option<IEnumerable<Err>>> Create(IDataType newItem)
    {
        var role = (Role)newItem;
        AppRole appRole = new(role.RolKey, role.RolName, role.RolLevel);
        //შევქმნათ როლი
        var result = await _roleManager.CreateAsync(appRole);
        role.RolId = appRole.Id;
        return (Err[])ConvertError(result);
    }

    public async ValueTask<Option<IEnumerable<Err>>> Update(int id, IDataType newItem)
    {
        var oldRole = await _roleManager.FindByIdAsync(id.ToString());
        if (oldRole == null)
            return new[] { MasterDataApiErrors.CannotFindRole };

        var role = (Role)newItem;
        oldRole.RoleName = role.RolName;
        oldRole.Level = role.RolLevel;

        var updateResult = await _roleManager.UpdateAsync(oldRole);
        if (!updateResult.Succeeded)
            return (Err[])ConvertError(updateResult);

        if (oldRole.RoleName == role.RolKey)
            return null;

        var setRoleResult = await _roleManager.SetRoleNameAsync(oldRole, role.RolKey);
        return (Err[])ConvertError(setRoleResult);
    }

    public async ValueTask<Option<IEnumerable<Err>>> Delete(int id)
    {
        var oldRole = await _roleManager.FindByIdAsync(id.ToString());
        if (oldRole == null)
            return new[] { MasterDataApiErrors.CannotFindRole };
        var deleteResult = await _roleManager.DeleteAsync(oldRole);
        return (Err[])ConvertError(deleteResult);
    }
}