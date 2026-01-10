using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using Carcass.Database.Models;
using CarcassIdentity;
using DomainShared.Repositories;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace CarcassRights;

public sealed class RightsDeterminer
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger _logger;
    private readonly IUserRightsRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    public RightsDeterminer(IUserRightsRepository repo, IUnitOfWork unitOfWork, ILogger logger,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Option<BadRequest<Err[]>>> CheckTableRights(string? userName, string method,
        TableKeyName tableKeyName, CancellationToken cancellationToken = default)
    {
        //var userName = _context.HttpContext.User.Identity?.Name;
        if (userName == null)
            return TypedResults.BadRequest(Err.Create(RightsApiErrors.UserNotIdentified));

        var tableKey = await tableKeyName.GetTableKey(_repo, cancellationToken);
        if (string.IsNullOrWhiteSpace(tableKey))
            return TypedResults.BadRequest(Err.Create(RightsApiErrors.TableNameNotIdentified));

        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
        var result = method == HttpMethods.Get
            ? await CheckViewRightByTableKey(tableKey, cancellationToken)
            : await CheckCrudRightByTableKey(tableKey, GetCrudType(method), cancellationToken);
        if (result.IsT1)
            return TypedResults.BadRequest(result.AsT1);

        //თუ არა დაბრუნდეს შეცდომა
        return !result.AsT0 ? TypedResults.BadRequest(Err.Create(RightsApiErrors.InsufficientRights)) : null;
    }

    private static Option<ECrudOperationType> GetCrudType(string method)
    {
        if (method == HttpMethods.Post)
            return ECrudOperationType.Create;
        if (method == HttpMethods.Put)
            return ECrudOperationType.Update;
        return method == HttpMethods.Delete ? ECrudOperationType.Delete : new Option<ECrudOperationType>();
    }

    public async ValueTask<OneOf<bool, Err[]>> CheckUserRightToClaim(string claimName,
        CancellationToken cancellationToken = default)
    {
        foreach (var role in _currentUser.Roles)
        {
            var result = await CheckRoleRightToClaim(role, claimName, cancellationToken);
            if (result.IsT0)
            {
                if (result.AsT0)
                    return true;
            }
            else
            {
                return result.AsT1;
            }
        }

        return false;
    }

    private async Task<OneOf<bool, Err[]>> CheckRoleRightToClaim(string roleName, string claimName,
        CancellationToken cancellationToken = default)
    {
        var roleDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var appClaimDataTypeId =
            await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<AppClaim>(), cancellationToken);

        if (roleDtId is null) _logger.LogError($"{nameof(CheckRoleRightToClaim)} {nameof(roleDtId)} is null");
        if (appClaimDataTypeId is null)
            _logger.LogError($"{nameof(CheckRoleRightToClaim)} {nameof(appClaimDataTypeId)} is null");

        if (roleDtId is null || appClaimDataTypeId is null)
            return new[] { RightsApiErrors.ErrorWhenDeterminingRights };

        return await _repo.CheckRight(roleDtId.Value, roleName, appClaimDataTypeId.Value, claimName, cancellationToken);
    }

    private async Task<OneOf<bool, Err[]>> CheckMenuRight(string roleName, string menuItemName,
        CancellationToken cancellationToken = default)
    {
        var menuGroupsDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<MenuGroup>(), cancellationToken);
        var menuDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<MenuItm>(), cancellationToken);
        var roleDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);

        if (menuGroupsDtId is null) _logger.LogError($"{nameof(CheckMenuRight)} {nameof(menuGroupsDtId)} is null");
        if (menuDtId is null) _logger.LogError($"{nameof(CheckMenuRight)} {nameof(menuDtId)} is null");
        if (roleDtId is null) _logger.LogError($"{nameof(CheckMenuRight)} {nameof(roleDtId)} is null");

        if (menuGroupsDtId is null || menuDtId is null || roleDtId is null)
            return new[] { RightsApiErrors.ErrorWhenDeterminingRights };

        return await _repo.CheckMenuRight(roleDtId.Value, roleName, menuGroupsDtId.Value, menuDtId.Value, menuItemName,
            cancellationToken);
    }

    public async ValueTask<OneOf<bool, Err[]>> HasUserRightRole(IEnumerable<string> menuNames,
        CancellationToken cancellationToken = default)
    {
        var menuNamesList = menuNames.ToList();
        var menuClaimCombo = from menuName in menuNamesList
            from roleName in _currentUser.Roles
            select new { menuName, roleName };
        List<Err> errors = [];

        foreach (var menuClaim in menuClaimCombo)
        {
            var result = await CheckMenuRight(menuClaim.roleName, menuClaim.menuName, cancellationToken);
            if (result.IsT0)
            {
                if (result.AsT0)
                    return true;
            }
            else
            {
                errors.AddRange(result.AsT1);
            }
        }

        if (errors.Count != 0)
            return errors.ToArray();
        return false;
    }

    private async ValueTask<OneOf<bool, Err[]>> CheckViewRightByTableKey(string tableKey,
        CancellationToken cancellationToken = default)
    {
        List<Err> errors = [];

        foreach (var roleName in _currentUser.Roles)
        {
            var result = await CheckViewRightByTableKey(roleName, tableKey, cancellationToken);

            if (result.IsT0)
            {
                if (result.AsT0)
                    return true;
            }
            else
            {
                errors.AddRange(result.AsT1);
            }
        }

        if (errors.Count != 0)
            return errors.ToArray();
        return false;
    }

    public async Task<OneOf<bool, Err[]>> CheckTableViewRight(string roleName, TableKeyName tableKeyName,
        CancellationToken cancellationToken = default)
    {
        var keyByTableName = await tableKeyName.GetTableKey(_repo, cancellationToken);
        if (keyByTableName is null) _logger.LogError($"{nameof(CheckTableViewRight)} {nameof(keyByTableName)} is null");

        if (keyByTableName is null)
            return new[] { RightsApiErrors.ErrorWhenDeterminingRights };

        return await CheckViewRightByTableKey(roleName, keyByTableName, cancellationToken);
    }

    private async Task<OneOf<bool, Err[]>> CheckViewRightByTableKey(string roleName, string tableKey,
        CancellationToken cancellationToken = default)
    {
        var roleDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var dataTypeDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<DataType>(), cancellationToken);
        var menuDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<MenuItm>(), cancellationToken);

        if (roleDtId is null) _logger.LogError($"{nameof(CheckViewRightByTableKey)} {nameof(roleDtId)} is null");
        if (dataTypeDtId is null)
            _logger.LogError($"{nameof(CheckViewRightByTableKey)} {nameof(dataTypeDtId)} is null");
        if (menuDtId is null) _logger.LogError($"{nameof(CheckViewRightByTableKey)} {nameof(menuDtId)} is null");

        if (roleDtId is null || dataTypeDtId is null || menuDtId is null)
            return new[] { RightsApiErrors.ErrorWhenDeterminingRights };

        return await _repo.CheckTableViewRight(roleDtId.Value, roleName, dataTypeDtId.Value, tableKey, menuDtId.Value,
            cancellationToken);
    }

    public async ValueTask<OneOf<bool, Err[]>> CheckTableListViewRight(IEnumerable<TableKeyName> tableKeysNames,
        CancellationToken cancellationToken = default)
    {
        var tableClaimCombo = from tableKeyName in tableKeysNames
            from roleName in _currentUser.Roles
            select new { tableKeyName, roleName };
        List<Err> errors = [];

        foreach (var menuClaim in tableClaimCombo)
        {
            var result = await CheckTableViewRight(menuClaim.roleName, menuClaim.tableKeyName, cancellationToken);
            if (result.IsT0)
            {
                if (result.AsT0)
                    return true;
            }
            else
            {
                errors.AddRange(result.AsT1);
            }
        }

        if (errors.Count != 0)
            return errors.ToArray();
        return false;
    }

    private async ValueTask<OneOf<bool, Err[]>> CheckCrudRightByTableKey(string tableKey,
        Option<ECrudOperationType> crudType, CancellationToken cancellationToken = default)
    {
        List<Err> errors = [];
        if (crudType.IsNone)
            return new[] { RightsApiErrors.ErrorWhenDeterminingCrudType };

        foreach (var roleName in _currentUser.Roles)
        {
            var result =
                await CheckCrudRightByTableKey(roleName, tableKey, (ECrudOperationType)crudType, cancellationToken);

            if (result.IsT0)
            {
                if (result.AsT0)
                    return true;
            }
            else
            {
                errors.AddRange(result.AsT1);
            }
        }

        if (errors.Count != 0)
            return errors.ToArray();
        return false;
    }

    private async Task<OneOf<bool, Err[]>> CheckCrudRightByTableKey(string roleName, string tableKey,
        ECrudOperationType crudType, CancellationToken cancellationToken = default)
    {
        var roleDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        var dataTypeDtId = await _repo.GetDataTypeIdByKey(_unitOfWork.GetTableName<DataType>(), cancellationToken);
        var dataCrudRightDtId = await _repo.GetDataTypeIdByKey(
            $"{_unitOfWork.GetTableName<DataType>()}{_unitOfWork.GetTableName<CrudRightType>()}", cancellationToken);

        if (roleDtId is null) _logger.LogError($"{nameof(CheckCrudRightByTableKey)} {nameof(roleDtId)} is null");
        if (dataTypeDtId is null)
            _logger.LogError($"{nameof(CheckCrudRightByTableKey)} {nameof(dataTypeDtId)} is null");
        if (dataCrudRightDtId is null)
            _logger.LogError($"{nameof(CheckCrudRightByTableKey)} {nameof(dataCrudRightDtId)} is null");
        //if (keyByTableName is null) _logger.LogError($"{nameof(CheckTableViewRight)} {nameof(keyByTableName)} is null");

        if (roleDtId is null || dataTypeDtId is null || dataCrudRightDtId is null)
            return new[] { RightsApiErrors.ErrorWhenDeterminingRights };

        return await _repo.CheckTableCrudRight(roleDtId.Value, roleName, dataTypeDtId.Value, tableKey,
            dataCrudRightDtId.Value, crudType, cancellationToken);
    }
}