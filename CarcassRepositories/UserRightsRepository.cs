using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassRights;
using Microsoft.EntityFrameworkCore;
using OneOf;
using RepositoriesAbstraction;
using SystemToolsShared.Errors;

namespace CarcassRepositories;

public sealed class UserRightsRepository : AbstractRepository, IUserRightsRepository
{
    private readonly CarcassDbContext _context;
    //private readonly IDataTypeKeys _dataTypeKeys;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserRightsRepository(CarcassDbContext context) : base(context)
    {
        _context = context;
        //_dataTypeKeys = dataTypeKeys;
    }

    public Task<bool> CheckRight(int parentDataTypeId, string parentKey, int childDataTypeId, string childKey,
        CancellationToken cancellationToken = default)
    {
        return _context.ManyToManyJoins.AnyAsync(
            w => w.PtId == parentDataTypeId && w.PKey == parentKey && w.CtId == childDataTypeId && w.CKey == childKey,
            cancellationToken);
    }

    public Task<bool> CheckMenuRight(int roleDtId, string roleName, int menuGroupsDtId, int menuDtId,
        string menuItemName, CancellationToken cancellationToken = default)
    {
        var res = from m in _context.Menu
            join mg in _context.MenuGroups on m.MenGroupId equals mg.MengId
            join rm in _context.ManyToManyJoins on m.MenKey equals rm.CKey
            join rmg in _context.ManyToManyJoins on mg.MengKey equals rmg.CKey
            where rm.PtId == roleDtId && rm.PKey == roleName && rm.CtId == menuDtId && rm.CKey == menuItemName &&
                  rmg.PtId == roleDtId && rmg.PKey == roleName && rmg.CtId == menuGroupsDtId
            select rm;

        return res.AnyAsync(cancellationToken);
    }

    public Task<string?> KeyByTableName(string tableName, CancellationToken cancellationToken = default)
    {
        return _context.DataTypes.Where(w => w.DtTable == tableName).Select(s => s.DtTable)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CheckTableViewRight(int roleDtId, string roleName, int dataTypeDtId, string keyByTableName,
        int menuDtId, CancellationToken cancellationToken = default)
    {
        //აქ GetManyToManyJoinsPccOne გამოყენებულია შემდეგი მიზნებისათვის:
        //უფლებების ცხრილში გვაქვს დარეგისტრირებული თუ მენიუს რომელ ელემენტს რომელი ცხრილები სჭირდება
        //თუ მომხმარებელს როლის მიხედვით უფლება აქვს მენიუს ელემენტზე, მაშინ ითვლება,
        //რომ მას ასევე უფლება აქვს ნახოს ყველა ის ცხრილი, რომელიც ამ მენიუს ელემენტს სჭირდება.
        //ამიტომ როლისათვის ვარკვევთ მენიუს გავლით რომელ ცხრილზე აქვს უფლებები ამ როლის მომხმარებელს.

        return await _context.ManyToManyJoins.AnyAsync(
            w => w.PtId == roleDtId && w.PKey == roleName && w.CtId == dataTypeDtId && w.CKey == keyByTableName,
            cancellationToken) || await GetManyToManyJoinsPccOne(roleDtId, roleName, menuDtId, dataTypeDtId,
            keyByTableName, cancellationToken);
    }

    public async Task<OneOf<bool, Err[]>> CheckTableCrudRight(int roleDtId, string roleName, int dataTypeDtId,
        string keyByTableName, int dataCrudRightDtId, ECrudOperationType crudType,
        CancellationToken cancellationToken = default)
    {
        return await _context.ManyToManyJoins.AnyAsync(
            w => w.PtId == roleDtId && w.PKey == roleName && w.CtId == dataTypeDtId && w.CKey == keyByTableName,
            cancellationToken) && _context.ManyToManyJoins.Any(w =>
            w.PtId == roleDtId && w.PKey == roleName && w.CtId == dataCrudRightDtId &&
            w.CKey == keyByTableName + '.' + Enum.GetName(typeof(ECrudOperationType), crudType));
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

    public async Task<int?> GetDataTypeIdByKey(string? tableName, CancellationToken cancellationToken = default)
    {
        return await _context.DataTypes.Where(w => w.DtTable == tableName).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private Task<bool> GetManyToManyJoinsPccOne(int parentTypeId, string parentKey, int childTypeId, int childTypeId2,
        string childKey2, CancellationToken cancellationToken = default)
    {
        return (from r1 in _context.ManyToManyJoins
            join r2 in _context.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals
                new { t = r2.PtId, i = r2.PKey }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId &&
                  r2.CtId == childTypeId2 && r2.CKey == childKey2
            select r2.CKey).AnyAsync(cancellationToken);
    }
}