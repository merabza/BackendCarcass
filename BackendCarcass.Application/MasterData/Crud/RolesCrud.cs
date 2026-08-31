using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Crud;
using BackendCarcass.Application.Crud.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Domain;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Application.MasterData.Crud;

public sealed class RolesCrud : CrudBase, IMasterDataLoader
{
    private readonly RoleManager<AppRole> _roleManager;
    private AppRole? _justCreated;

    public RolesCrud(ILogger logger, RoleManager<AppRole> roleManager, IUnitOfWork unitOfWork,
        IDatabaseAbstraction databaseAbstraction) : base(logger, unitOfWork, databaseAbstraction)
    {
        _roleManager = roleManager;
    }

    protected override int JustCreatedId => _justCreated?.Id ?? 0;

    public async ValueTask<Result<IEnumerable<IDataType>>> GetAllRecords(CancellationToken cancellationToken = default)
    {
        List<AppRole> roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        return Result.Success<IEnumerable<IDataType>>(roles.Select(x =>
            new RoleCrudData(x.Name ?? x.RoleName, x.RoleName, x.Level)));
    }

    public override async ValueTask<Result<TableRowsData>> GetTableRowsData(FilterSortRequest filterSortRequest,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AppRole> roles = _roleManager.Roles;

        (int realOffset, int count, List<RoleCrudData> rows) = await roles.UseCustomSortFilterPagination(
            filterSortRequest, x => new RoleCrudData(x.Name ?? x.RoleName, x.RoleName, x.Level), cancellationToken);

        return new TableRowsData(count, realOffset, [.. rows.Select(s => s.EditFields())]);
    }

    protected override async Task<Result<ICrudData>> GetOneData(int id, CancellationToken cancellationToken = default)
    {
        AppRole? appRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (appRole?.Name is not null)
        {
            return new RoleCrudData(appRole.Name, appRole.RoleName, appRole.Level);
        }

        return Result.Failure<ICrudData>(MasterDataApiErrors.CannotFindRole);
    }

    protected override async ValueTask<Result> CreateData(ICrudData crudDataForCreate,
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
        return Result.Success();
    }

    protected override async ValueTask<Result> UpdateData(int id, ICrudData crudDataNewVersion,
        CancellationToken cancellationToken = default)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole is null)
        {
            return Result.Failure(MasterDataApiErrors.CannotFindRole);
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
            return Result.Success();
        }

        IdentityResult setRoleResult = await _roleManager.SetRoleNameAsync(oldRole, role.RolKey);
        return ConvertError(setRoleResult);
    }

    protected override async Task<Result> DeleteData(int id, CancellationToken cancellationToken = default)
    {
        AppRole? oldRole = await _roleManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture));
        if (oldRole is null)
        {
            return Result.Failure(MasterDataApiErrors.CannotFindRole);
        }

        IdentityResult deleteResult = await _roleManager.DeleteAsync(oldRole);
        return ConvertError(deleteResult);
    }

    private static Result ConvertError(IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(Result.CreateValidationError([
                .. result.Errors.Select(s => Error.Problem(s.Code, s.Description))
            ]));
    }
}
