using System.Collections.Generic;
using System.Threading.Tasks;
using CarcassMasterDataDom;
using OneOf;
using SystemToolsShared;

namespace CarcassRightsDom;

public interface IUserRightsRepository
{
    //Task<bool> CheckUserRightToClaim(IEnumerable<Claim> userClaims, string claimName);
    Task<int?> GetDataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey);
    Task<bool> CheckRight(int parentDataTypeId, string parentKey, int childDataTypeId, string childKey);
    Task<bool> CheckMenuRight(int roleDtId, string roleName, int menuGroupsDtId, int menuDtId, string menuItemName);
    Task<string?> KeyByTableName(string tableName);

    Task<bool> CheckTableViewRight(int roleDtId, string roleName, int dataTypeDtId, string keyByTableName,
        int menuDtId);

    Task<OneOf<bool, IEnumerable<Err>>> CheckTableCrudRight(int roleDtId, string roleName, int dataTypeDtId,
        string keyByTableName, int dataCrudRightDtId, ECrudOperationType crudType);
}