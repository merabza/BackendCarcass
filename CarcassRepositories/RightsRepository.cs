using System.Linq;
using CarcassDom;
using CarcassMasterDataDom;
using System.Threading.Tasks;
using System.Threading;
using CarcassDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using CarcassDom.Models;
using CarcassMappers;
using CarcassMasterDataDom.Models;

namespace CarcassRepositories;

public sealed class RightsRepository : IRightsRepository
{
    private readonly CarcassDbContext _carcassContext;
    private readonly ILogger<RightsRepository> _logger;

    public RightsRepository(CarcassDbContext context, ILogger<RightsRepository> logger,
        IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _carcassContext = context;
        _logger = logger;
    }

    public async Task<int> DataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken)
    {
        return await _carcassContext.DataTypes.Where(w => w.DtKey == dataTypeKey.ToDtKey()).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public int UserMinLevel(IEnumerable<string> drPcs)
    {
        return (from drPc in drPcs
            join rol in _carcassContext.Roles on drPc equals rol.RolKey
            select rol.RolLevel).DefaultIfEmpty(1000).Min();
    }

    public async Task<List<Tuple<int, int>>> UsersMinLevels(int roleDataId, int userDataId,
        CancellationToken cancellationToken)
    {
        return await (from dr in _carcassContext.ManyToManyJoins
                join usr in _carcassContext.Users on new { a = dr.PtId, b = dr.PKey } equals
                    new { a = userDataId, b = usr.UserName }
                join rol in _carcassContext.Roles on new { a = dr.CtId, b = dr.CKey } equals
                    new { a = roleDataId, b = rol.RolKey }
                group rol by usr.UsrId
                into ug
                select new Tuple<int, int>(ug.Key, ug.Min(usr => usr.RolLevel)))
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<UserModel>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _carcassContext.Users.ToListAsync(cancellationToken);
        return users.Select(x => x.AdaptTo()).ToList();
    }


    public async Task<List<ReturnValueModel>> GetRoleReturnValues(int minLevel, CancellationToken cancellationToken)
    {
        return await _carcassContext.Roles.Where(w => w.RolLevel >= minLevel)
            .Select(role => new ReturnValueModel { Value = role.RolId, Key = role.RolKey, Name = role.RolName })
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public IQueryable<string> ManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.PtId == parentTypeId && w.PKey == parentKey && w.CtId == childTypeId).Select(s => s.CKey);
        //.ToListAsync(cancellationToken: cancellationToken);
    }

    public IQueryable<string> ManyToManyJoinsPcc2(int parentTypeId, string parentKey, int childTypeId,
        int mmjDataId, int childTypeId2, int childTypeId3)
    {
        return from r in _carcassContext.ManyToManyJoins
            join r1 in _carcassContext.ManyToManyJoins on new { t = r.PtId, i = r.PKey } equals new
                { t = r1.CtId, i = r1.CKey }
            join drt in _carcassContext.ManyToManyJoins on r.CKey equals drt.PKey + "." + drt.CKey
            where r.CtId == mmjDataId && r.PtId == childTypeId && r1.PtId == parentTypeId &&
                  r1.PKey == parentKey && drt.PtId == childTypeId2 && drt.CtId == childTypeId3
            select drt.PKey;
    }

    private IQueryable<string> ManyToManyJoinsPcc3(int parentTypeId, string parentKey, int childTypeId,
        int mmjDataId, int childTypeId2, int childTypeId3)
    {
        return from r in _carcassContext.ManyToManyJoins
            join r1 in _carcassContext.ManyToManyJoins on new { t = r.PtId, i = r.PKey } equals new
                { t = r1.CtId, i = r1.CKey }
            join drt in _carcassContext.ManyToManyJoins on r.CKey equals drt.PKey + "." + drt.CKey
            where r.CtId == mmjDataId && r.PtId == childTypeId && r1.PtId == parentTypeId &&
                  r1.PKey == parentKey && drt.PtId == childTypeId2 && drt.CtId == childTypeId3
            select drt.CKey;
    }

    public async Task<List<DataTypeModelForRvs>> ParentsDataTypesNormalView(int dtDataId, string dataTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken)
    {
        var result = from dt in _carcassContext.DataTypes
            join pc in ManyToManyJoinsPc(dtDataId, dataTypeKey, dtDataId) on dt.DtKey equals pc
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey equals pcc2
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    public async Task<List<DataTypeModelForRvs>> ParentsDataTypesReverseView(int dtDataId, int userDataId, string userName,
        int roleDataId, int mmjDataId, CancellationToken cancellationToken)
    {
        var result = from dt in _carcassContext.DataTypes
            join dr in _carcassContext.ManyToManyJoins on new { a = dt.DtKey, b = dtDataId, c = dtDataId } equals new
            {
                a = dr.CKey,
                b = dr.PtId,
                c = dr.CtId
            }
            join pcc3 in ManyToManyJoinsPcc3(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey
                equals
                pcc3
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    public async Task<List<DataTypeModelForRvs>> ChildrenDataTypesNormalView(int dtDataId, string parentTypeKey,
        int userDataId, string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken)
    {
        var result = from dt in _carcassContext.DataTypes
            join pc in ManyToManyJoinsPc(dtDataId, parentTypeKey, dtDataId) on dt.DtKey equals pc
            join pcc3 in ManyToManyJoinsPcc3(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey
                equals
                pcc3
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    public async Task<List<DataTypeModelForRvs>> ChildrenDataTypesReverseView(int dtDataId, string parentTypeKey,
        int userDataId, string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken)
    {
        var result = from dt in _carcassContext.DataTypes
            join cp in ManyToManyJoinsCp(dtDataId, parentTypeKey, dtDataId) on dt.DtKey equals cp
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey
                equals
                pcc2
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    private IQueryable<string> ManyToManyJoinsCp(int childTypeId, string childKey, int parentTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.CtId == childTypeId && w.CKey == childKey && w.PtId == parentTypeId).Select(s => s.PKey);
    }


}