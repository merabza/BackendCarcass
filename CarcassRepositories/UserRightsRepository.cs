using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassDom;
using CarcassMasterDataDom;
using Microsoft.EntityFrameworkCore;
using OneOf;
using SystemToolsShared.Errors;

namespace CarcassRepositories;

public class UserRightsRepository : IUserRightsRepository
{
    private readonly CarcassDbContext _context;
    //private readonly IDataTypeKeys _dataTypeKeys;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserRightsRepository(CarcassDbContext context)
    {
        _context = context;
        //_dataTypeKeys = dataTypeKeys;
    }

    //public bool CheckUserRightToClaim(IEnumerable<Claim> userClaims, string claimName)
    //{
    //    return GetRoles(userClaims).Any(roleName => CheckRoleRightToClaim(roleName, claimName));
    //}

    //private bool CheckRoleRightToClaim(string roleName, string claimName)
    //{
    //    var roleDtId = GetDataTypeIdByKey(ECarcassDataTypeKeys.Role);
    //    var appClaimDataTypeId = GetDataTypeIdByKey(ECarcassDataTypeKeys.AppClaim);

    //    return _context.ManyToManyJoins.Any(w =>
    //        w.PtId == roleDtId && w.PKey == roleName && w.CtId == appClaimDataTypeId && w.CKey == claimName);
    //}

    public async Task<int?> GetDataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken)
    {
        return await _context.DataTypes.Where(w => w.DtKey == dataTypeKey.ToDtKey()).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CheckRight(int parentDataTypeId, string parentKey, int childDataTypeId, string childKey,
        CancellationToken cancellationToken)
    {
        return await _context.ManyToManyJoins.AnyAsync(
            w => w.PtId == parentDataTypeId && w.PKey == parentKey && w.CtId == childDataTypeId && w.CKey == childKey,
            cancellationToken);
    }


    public async Task<bool> CheckMenuRight(int roleDtId, string roleName, int menuGroupsDtId, int menuDtId,
        string menuItemName, CancellationToken cancellationToken)
    {
        var res = from m in _context.Menu
            join mg in _context.MenuGroups on m.MenGroupId equals mg.MengId
            join rm in _context.ManyToManyJoins on m.MenKey equals rm.CKey
            join rmg in _context.ManyToManyJoins on mg.MengKey equals rmg.CKey
            where rm.PtId == roleDtId && rm.PKey == roleName && rm.CtId == menuDtId && rm.CKey == menuItemName &&
                  rmg.PtId == roleDtId && rmg.PKey == roleName && rmg.CtId == menuGroupsDtId
            select rm;

        return await res.AnyAsync(cancellationToken);
    }

    public async Task<string?> KeyByTableName(string tableName, CancellationToken cancellationToken)
    {
        return await _context.DataTypes.Where(w => w.DtTable == tableName).Select(s => s.DtKey)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CheckTableViewRight(int roleDtId, string roleName, int dataTypeDtId, string keyByTableName,
        int menuDtId, CancellationToken cancellationToken)
    {
        //აქ GetManyToManyJoinsPccOne გამოყენებულია შემდეგი მიზნებისათვის:
        //უფლებების ცხრილში გვაქვს დარეგისტრირებული თუ მენიუს რომელ ელემენტს რომელი ცხრილები სჭირდება
        //თუ მომხმარებელს როლის მიხედვით უფლება აქვს მენიუს ელემენტზე, მაშინ ითვლება,
        //რომ მას ასევე უფლება აქვს ნახოს ყველა ის ცხრილი, რომელიც ამ მენიუს ელემენტს სჭირდება.
        //ამიტომ როლისათვის ვარკვევთ მენიუს გავლით რომელ ცხრილზე აქვს უფლებები ამ როლის მომხმარებელს.

        return await _context.ManyToManyJoins.AnyAsync(w =>
                       w.PtId == roleDtId && w.PKey == roleName && w.CtId == dataTypeDtId && w.CKey == keyByTableName,
                   cancellationToken) ||
               await GetManyToManyJoinsPccOne(roleDtId, roleName, menuDtId, dataTypeDtId, keyByTableName,
                   cancellationToken);
    }

    public async Task<OneOf<bool, IEnumerable<Err>>> CheckTableCrudRight(int roleDtId, string roleName,
        int dataTypeDtId, string keyByTableName, int dataCrudRightDtId,
        ECrudOperationType crudType, CancellationToken cancellationToken)
    {
        return await _context.ManyToManyJoins.AnyAsync(w =>
                       w.PtId == roleDtId && w.PKey == roleName && w.CtId == dataTypeDtId && w.CKey == keyByTableName,
                   cancellationToken) &&
               _context.ManyToManyJoins.Any(w =>
                   w.PtId == roleDtId && w.PKey == roleName && w.CtId == dataCrudRightDtId &&
                   w.CKey == keyByTableName + '.' + Enum.GetName(typeof(ECrudOperationType), crudType));
    }

    private async Task<bool> GetManyToManyJoinsPccOne(int parentTypeId, string parentKey, int childTypeId,
        int childTypeId2, string childKey2, CancellationToken cancellationToken)
    {
        return await (from r1 in _context.ManyToManyJoins
            join r2 in _context.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
                { t = r2.PtId, i = r2.PKey }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId &&
                  r2.CtId == childTypeId2 &&
                  r2.CKey == childKey2
            select r2.CKey).AnyAsync(cancellationToken);
    }
}