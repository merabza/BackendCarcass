using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.LibCrud.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.Domain.Abstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.Crud;

public sealed class RolesCrud : CrudBase, IMasterDataLoader
{
    private readonly RoleManager<AppRole> _roleManager;
    private AppRole? _justCreated;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RolesCrud(ILogger logger, RoleManager<AppRole> roleManager, IUnitOfWork unitOfWork,
        IDatabaseAbstraction databaseAbstraction) : base(logger, unitOfWork, databaseAbstraction)
    {
        _roleManager = roleManager;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async ValueTask<OneOf<IEnumerable<IDataType>, Error[]>> GetAllRecords(
        CancellationToken cancellationToken = default)
    {
        List<AppRole> roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        return OneOf<IEnumerable<IDataType>, Error[]>.FromT0(roles.Select(x =>
            new RoleCrudData(x.Name ?? x.RoleName, x.RoleName, x.Level)));
    }

    public override async ValueTask<OneOf<TableRowsData, Error[]>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AppRole> roles = _roleManager.Roles;

        (int realOffset, int count, List<RoleCrudData> rows) = await roles.UseCustomSortFilterPagination(
            filterSortRequest, x => new RoleCrudData(x.Name ?? x.RoleName, x.RoleName, x.Level), cancellationToken);

        return new TableRowsData(count, realOffset, rows.Select(s => s.EditFields()).ToList());
    }

    protected override async Task<OneOf<ICrudData, Error[]>> GetOneData(int id,
        CancellationToken cancellationToken = default)
    {
        AppRole? appRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (appRole?.Name is not null)
        {
            return new RoleCrudData(appRole.Name, appRole.RoleName, appRole.Level);
        }

        return new[] { MasterDataApiErrors.CannotFindRole };
    }

    protected override async ValueTask<Option<Error[]>> CreateData(ICrudData crudDataForCreate,
        CancellationToken cancellationToken = default)
    {
        var role = (RoleCrudData)crudDataForCreate;
        var appRole = new AppRole(role.RolKey, role.RolName, role.RolLevel);
        //შევქმნათ როლი
        IdentityResult createResult = await _roleManager.CreateAsync(appRole);
        if (!createResult.Succeeded)
        {
            return ConvertError(createResult);
        }

        _justCreated = appRole;
        return null;
    }

    protected override async ValueTask<Option<Error[]>> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole is null)
        {
            return new[] { MasterDataApiErrors.CannotFindRole };
        }

        var role = (RoleCrudData)crudDataNewVersion;
        oldRole.RoleName = role.RolName;
        oldRole.Level = role.RolLevel;

        IdentityResult updateResult = await _roleManager.UpdateAsync(oldRole);
        if (!updateResult.Succeeded)
        {
            return ConvertError(updateResult);
        }

        if (oldRole.RoleName == role.RolKey)
        {
            return null;
        }

        IdentityResult setRoleResult = await _roleManager.SetRoleNameAsync(oldRole, role.RolKey);
        return ConvertError(setRoleResult);
    }

    protected override async Task<Option<Error[]>> DeleteData(int id, CancellationToken cancellationToken = default)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole is null)
        {
            return new[] { MasterDataApiErrors.CannotFindRole };
        }

        IdentityResult deleteResult = await _roleManager.DeleteAsync(oldRole);
        return ConvertError(deleteResult);
    }

    private static Option<Error[]> ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? null
            : result.Errors.Select(x => new Error { Code = x.Code, Name = x.Description }).ToArray();
    }
}
