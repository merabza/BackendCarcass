using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.LibCrud.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcassContracts.Errors;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.DomainShared.Repositories;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Crud;

public sealed class RolesCrud : CrudBase, IMasterDataLoader
{
    private readonly RoleManager<AppRole> _roleManager;
    private AppRole? _justCreated;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RolesCrud(ILogger logger, RoleManager<AppRole> roleManager, IUnitOfWork unitOfWork) : base(logger,
        unitOfWork)
    {
        _roleManager = roleManager;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async ValueTask<OneOf<IEnumerable<IDataType>, Err[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Err[]>.FromT0(roles.Select(x =>
            new RoleCrudData(x.Name ?? x.RoleName, x.RoleName, x.Level)));
    }

    public override async ValueTask<OneOf<TableRowsData, Err[]>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken = default)
    {
        var roles = _roleManager.Roles;

        var (realOffset, count, rows) = await roles.UseCustomSortFilterPagination(filterSortRequest,
            x => new RoleCrudData(x.Name ?? x.RoleName, x.RoleName, x.Level), cancellationToken);

        return new TableRowsData(count, realOffset, rows.Select(s => s.EditFields()).ToList());
    }

    protected override async Task<OneOf<ICrudData, Err[]>> GetOneData(int id,
        CancellationToken cancellationToken = default)
    {
        var appRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (appRole?.Name is not null)
        {
            return new RoleCrudData(appRole.Name, appRole.RoleName, appRole.Level);
        }

        return new[] { MasterDataApiErrors.CannotFindRole };
    }

    protected override async ValueTask<Option<Err[]>> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default)
    {
        var role = (RoleCrudData)crudDataForCreate;
        var appRole = new AppRole(role.RolKey, role.RolName, role.RolLevel);
        //შევქმნათ როლი
        var createResult = await _roleManager.CreateAsync(appRole);
        if (!createResult.Succeeded)
        {
            return ConvertError(createResult);
        }

        _justCreated = appRole;
        return null;
    }

    protected override async ValueTask<Option<Err[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default)
    {
        var oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole is null)
        {
            return new[] { MasterDataApiErrors.CannotFindRole };
        }

        var role = (RoleCrudData)crudDataNewVersion;
        oldRole.RoleName = role.RolName;
        oldRole.Level = role.RolLevel;

        var updateResult = await _roleManager.UpdateAsync(oldRole);
        if (!updateResult.Succeeded)
        {
            return ConvertError(updateResult);
        }

        if (oldRole.RoleName == role.RolKey)
        {
            return null;
        }

        var setRoleResult = await _roleManager.SetRoleNameAsync(oldRole, role.RolKey);
        return ConvertError(setRoleResult);
    }

    protected override async Task<Option<Err[]>> DeleteData(int id, CancellationToken cancellationToken = default)
    {
        var oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole is null)
        {
            return new[] { MasterDataApiErrors.CannotFindRole };
        }

        var deleteResult = await _roleManager.DeleteAsync(oldRole);
        return ConvertError(deleteResult);
    }

    private static Option<Err[]> ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? null
            : result.Errors.Select(x => new Err { ErrorCode = x.Code, ErrorMessage = x.Description }).ToArray();
    }
}
