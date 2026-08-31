using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Domain.AppClaims;
using BackendCarcass.Domain.CrudRightTypes;
using BackendCarcass.Domain.DataTypes;
using BackendCarcass.Domain.MenuGroups;
using BackendCarcass.Domain.MenuItems;
using BackendCarcass.Domain.Roles;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.Rights;

public sealed class RightsDeterminer
{
    private readonly ICurrentUser _currentUser;
    private readonly IDatabaseAbstraction _databaseAbstraction;
    private readonly ILogger _logger;
    private readonly IUserRightsRepository _repo;

    public RightsDeterminer(IUserRightsRepository repo, ILogger logger, ICurrentUser currentUser,
        IDatabaseAbstraction databaseAbstraction)
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
        _databaseAbstraction = databaseAbstraction;
    }

    public async ValueTask<Result> CheckTableRights(string? userName, string method, TableKeyName tableKeyName,
        CancellationToken cancellationToken = default)
    {
        //var userName = _context.HttpContext.User.Identity?.Name;
        if (userName == null)
        {
            return Result.Failure(RightsApiErrors.UserNotIdentified);
        }

        string? tableKey = await tableKeyName.GetTableKey(_repo, cancellationToken);
        if (string.IsNullOrWhiteSpace(tableKey))
        {
            return Result.Failure(RightsApiErrors.TableNameNotIdentified);
        }

        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
        Result<bool> result = method == HttpMethods.Get
            ? await CheckViewRightByTableKey(tableKey, cancellationToken)
            : await CheckCrudRightByTableKey(tableKey, GetCrudType(method), cancellationToken);
        if (result.IsFailure)
        {
            return result;
        }

        //თუ არა დაბრუნდეს შეცდომა
        return !result.Value ? Result.Failure(RightsApiErrors.InsufficientRights) : Result.Success();
    }

    private static Option<ECrudOperationType> GetCrudType(string method)
    {
        if (method == HttpMethods.Post)
        {
            return ECrudOperationType.Create;
        }

        if (method == HttpMethods.Put)
        {
            return ECrudOperationType.Update;
        }

        return method == HttpMethods.Delete ? ECrudOperationType.Delete : new Option<ECrudOperationType>();
    }

    public async ValueTask<Result<bool>> CheckUserRightToClaim(string claimName,
        CancellationToken cancellationToken = default)
    {
        foreach (string role in _currentUser.Roles)
        {
            Result<bool> result = await CheckRoleRightToClaim(role, claimName, cancellationToken);
            if (result.IsFailure)
            {
                return result;
            }

            if (result.Value)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<Result<bool>> CheckRoleRightToClaim(string roleName, string claimName,
        CancellationToken cancellationToken = default)
    {
        int? roleDtId = await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<Role>(), cancellationToken);
        int? appClaimDataTypeId =
            await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<AppClaim>(), cancellationToken);

        if (roleDtId is null)
        {
            _logger.LogError($"{nameof(CheckRoleRightToClaim)} {nameof(roleDtId)} is null");
        }

        if (appClaimDataTypeId is null)
        {
            _logger.LogError($"{nameof(CheckRoleRightToClaim)} {nameof(appClaimDataTypeId)} is null");
        }

        if (roleDtId is null || appClaimDataTypeId is null)
        {
            return Result.Failure<bool>(RightsApiErrors.ErrorWhenDeterminingRights);
        }

        return await _repo.CheckRight(roleDtId.Value, roleName, appClaimDataTypeId.Value, claimName, cancellationToken);
    }

    private async Task<Result<bool>> CheckMenuRight(string roleName, string menuItemName,
        CancellationToken cancellationToken = default)
    {
        int? menuGroupsDtId =
            await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<MenuGroup>(), cancellationToken);
        int? menuDtId =
            await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<MenuItem>(), cancellationToken);
        int? roleDtId = await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<Role>(), cancellationToken);

        if (menuGroupsDtId is null)
        {
            _logger.LogError($"{nameof(CheckMenuRight)} {nameof(menuGroupsDtId)} is null");
        }

        if (menuDtId is null)
        {
            _logger.LogError($"{nameof(CheckMenuRight)} {nameof(menuDtId)} is null");
        }

        if (roleDtId is null)
        {
            _logger.LogError($"{nameof(CheckMenuRight)} {nameof(roleDtId)} is null");
        }

        if (menuGroupsDtId is null || menuDtId is null || roleDtId is null)
        {
            return Result.Failure<bool>(RightsApiErrors.ErrorWhenDeterminingRights);
        }

        return await _repo.CheckMenuRight(roleDtId.Value, roleName, menuGroupsDtId.Value, menuDtId.Value, menuItemName,
            cancellationToken);
    }

    public async ValueTask<Result<bool>> HasUserRightRole(IEnumerable<string> menuNames,
        CancellationToken cancellationToken = default)
    {
        List<string> menuNamesList = [.. menuNames];
        var menuClaimCombo = from menuName in menuNamesList
            from roleName in _currentUser.Roles
            select new { menuName, roleName };
        List<Error> errors = [];

        foreach (var menuClaim in menuClaimCombo)
        {
            Result<bool> result = await CheckMenuRight(menuClaim.roleName, menuClaim.menuName, cancellationToken);
            if (result.IsFailure)
            {
                errors.Add(result.Error);
            }
            else if (result.Value)
            {
                return true;
            }
        }

        if (errors.Count != 0)
        {
            return Result.Failure<bool>(CombineErrors(errors));
        }

        return false;
    }

    private async ValueTask<Result<bool>> CheckViewRightByTableKey(string tableKey,
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];

        foreach (string roleName in _currentUser.Roles)
        {
            Result<bool> result = await CheckViewRightByTableKey(roleName, tableKey, cancellationToken);

            if (result.IsFailure)
            {
                errors.Add(result.Error);
            }
            else if (result.Value)
            {
                return true;
            }
        }

        if (errors.Count != 0)
        {
            return Result.Failure<bool>(CombineErrors(errors));
        }

        return false;
    }

    public async Task<Result<bool>> CheckTableViewRight(string roleName, TableKeyName tableKeyName,
        CancellationToken cancellationToken = default)
    {
        string? keyByTableName = await tableKeyName.GetTableKey(_repo, cancellationToken);
        if (keyByTableName is null)
        {
            _logger.LogError($"{nameof(CheckTableViewRight)} {nameof(keyByTableName)} is null");
        }

        if (keyByTableName is null)
        {
            return Result.Failure<bool>(RightsApiErrors.ErrorWhenDeterminingRights);
        }

        return await CheckViewRightByTableKey(roleName, keyByTableName, cancellationToken);
    }

    private async Task<Result<bool>> CheckViewRightByTableKey(string roleName, string tableKey,
        CancellationToken cancellationToken = default)
    {
        int? roleDtId = await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<Role>(), cancellationToken);
        int? dataTypeDtId =
            await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<DataType>(), cancellationToken);
        int? menuDtId =
            await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<MenuItem>(), cancellationToken);

        if (roleDtId is null)
        {
            _logger.LogError($"{nameof(CheckViewRightByTableKey)} {nameof(roleDtId)} is null");
        }

        if (dataTypeDtId is null)
        {
            _logger.LogError($"{nameof(CheckViewRightByTableKey)} {nameof(dataTypeDtId)} is null");
        }

        if (menuDtId is null)
        {
            _logger.LogError($"{nameof(CheckViewRightByTableKey)} {nameof(menuDtId)} is null");
        }

        if (roleDtId is null || dataTypeDtId is null || menuDtId is null)
        {
            return Result.Failure<bool>(RightsApiErrors.ErrorWhenDeterminingRights);
        }

        return await _repo.CheckTableViewRight(roleDtId.Value, roleName, dataTypeDtId.Value, tableKey, menuDtId.Value,
            cancellationToken);
    }

    public async ValueTask<Result<bool>> CheckTableListViewRight(IEnumerable<TableKeyName> tableKeysNames,
        CancellationToken cancellationToken = default)
    {
        var tableClaimCombo = from tableKeyName in tableKeysNames
            from roleName in _currentUser.Roles
            select new { tableKeyName, roleName };
        List<Error> errors = [];

        foreach (var menuClaim in tableClaimCombo)
        {
            Result<bool> result =
                await CheckTableViewRight(menuClaim.roleName, menuClaim.tableKeyName, cancellationToken);
            if (result.IsFailure)
            {
                errors.Add(result.Error);
            }
            else if (result.Value)
            {
                return true;
            }
        }

        if (errors.Count != 0)
        {
            return Result.Failure<bool>(CombineErrors(errors));
        }

        return false;
    }

    private async ValueTask<Result<bool>> CheckCrudRightByTableKey(string tableKey, Option<ECrudOperationType> crudType,
        CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];
        if (crudType.IsNone)
        {
            return Result.Failure<bool>(RightsApiErrors.ErrorWhenDeterminingCrudType);
        }

        foreach (string roleName in _currentUser.Roles)
        {
            Result<bool> result =
                await CheckCrudRightByTableKey(roleName, tableKey, (ECrudOperationType)crudType, cancellationToken);

            if (result.IsFailure)
            {
                errors.Add(result.Error);
            }
            else if (result.Value)
            {
                return true;
            }
        }

        if (errors.Count != 0)
        {
            return Result.Failure<bool>(CombineErrors(errors));
        }

        return false;
    }

    private async Task<Result<bool>> CheckCrudRightByTableKey(string roleName, string tableKey,
        ECrudOperationType crudType, CancellationToken cancellationToken = default)
    {
        int? roleDtId = await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<Role>(), cancellationToken);
        int? dataTypeDtId =
            await _repo.GetDataTypeIdByKey(_databaseAbstraction.GetTableName<DataType>(), cancellationToken);
        int? dataCrudRightDtId = await _repo.GetDataTypeIdByKey(
            $"{_databaseAbstraction.GetTableName<DataType>()}{_databaseAbstraction.GetTableName<CrudRightType>()}",
            cancellationToken);

        if (roleDtId is null)
        {
            _logger.LogError($"{nameof(CheckCrudRightByTableKey)} {nameof(roleDtId)} is null");
        }

        if (dataTypeDtId is null)
        {
            _logger.LogError($"{nameof(CheckCrudRightByTableKey)} {nameof(dataTypeDtId)} is null");
        }

        if (dataCrudRightDtId is null)
        {
            _logger.LogError($"{nameof(CheckCrudRightByTableKey)} {nameof(dataCrudRightDtId)} is null");
        }
        //if (keyByTableName is null) _logger.LogError($"{nameof(CheckTableViewRight)} {nameof(keyByTableName)} is null");

        if (roleDtId is null || dataTypeDtId is null || dataCrudRightDtId is null)
        {
            return Result.Failure<bool>(RightsApiErrors.ErrorWhenDeterminingRights);
        }

        return await _repo.CheckTableCrudRight(roleDtId.Value, roleName, dataTypeDtId.Value, tableKey,
            dataCrudRightDtId.Value, crudType, cancellationToken);
    }

    private static Error CombineErrors(List<Error> errors)
    {
        return errors.Count == 1 ? errors[0] : new ValidationError([.. errors]);
    }
}
