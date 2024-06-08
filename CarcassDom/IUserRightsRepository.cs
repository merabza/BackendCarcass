using CarcassMasterDataDom;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared.Errors;

namespace CarcassDom;

public interface IUserRightsRepository
{
    //Task<bool> CheckUserRightToClaim(IEnumerable<Claim> userClaims, string claimName);
    Task<int?> GetDataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken);

    Task<bool> CheckRight(int parentDataTypeId, string parentKey, int childDataTypeId, string childKey,
        CancellationToken cancellationToken);

    Task<bool> CheckMenuRight(int roleDtId, string roleName, int menuGroupsDtId, int menuDtId, string menuItemName,
        CancellationToken cancellationToken);

    Task<string?> KeyByTableName(string tableName, CancellationToken cancellationToken);

    Task<bool> CheckTableViewRight(int roleDtId, string roleName, int dataTypeDtId, string keyByTableName, int menuDtId,
        CancellationToken cancellationToken);

    Task<OneOf<bool, IEnumerable<Err>>> CheckTableCrudRight(int roleDtId, string roleName, int dataTypeDtId,
        string keyByTableName, int dataCrudRightDtId, ECrudOperationType crudType, CancellationToken cancellationToken);
}