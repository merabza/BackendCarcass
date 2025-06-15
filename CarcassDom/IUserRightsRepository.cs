using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using RepositoriesDom;
using SystemToolsShared.Errors;

namespace CarcassDom;

public interface IUserRightsRepository : IAbstractRepository
{
    //Task<bool> CheckUserRightToClaim(IEnumerable<Claim> userClaims, string claimName);
    Task<int?> GetDataTypeIdByKey(string? tableName, CancellationToken cancellationToken = default);

    Task<bool> CheckRight(int parentDataTypeId, string parentKey, int childDataTypeId, string childKey,
        CancellationToken cancellationToken = default);

    Task<bool> CheckMenuRight(int roleDtId, string roleName, int menuGroupsDtId, int menuDtId, string menuItemName,
        CancellationToken cancellationToken = default);

    Task<string?> KeyByTableName(string tableName, CancellationToken cancellationToken = default);

    Task<bool> CheckTableViewRight(int roleDtId, string roleName, int dataTypeDtId, string keyByTableName, int menuDtId,
        CancellationToken cancellationToken = default);

    Task<OneOf<bool, IEnumerable<Err>>> CheckTableCrudRight(int roleDtId, string roleName, int dataTypeDtId,
        string keyByTableName, int dataCrudRightDtId, ECrudOperationType crudType,
        CancellationToken cancellationToken = default);
}