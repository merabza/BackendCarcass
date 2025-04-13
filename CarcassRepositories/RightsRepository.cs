using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassDb.Models;
using CarcassDom;
using CarcassDom.Models;
using CarcassMappers;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoriesDom;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace CarcassRepositories;

public sealed class RightsRepository : AbstractRepository, IRightsRepository
{
    private readonly CarcassDbContext _carcassContext;
    private readonly ILogger<RightsRepository> _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RightsRepository(ILogger<RightsRepository> logger, CarcassDbContext context) : base(context)
    {
        _carcassContext = context;
        _logger = logger;
    }

    public Task<int> DataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken = default)
    {
        return _carcassContext.DataTypes.Where(w => w.DtKey == dataTypeKey.ToDtKey()).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public int UserMinLevel(IEnumerable<string> drPcs)
    {
        return (from drPc in drPcs join rol in _carcassContext.Roles on drPc equals rol.RolKey select rol.RolLevel)
            .DefaultIfEmpty(1000).Min();
    }

    public Task<List<Tuple<int, int>>> UsersMinLevels(int roleDataId, int userDataId,
        CancellationToken cancellationToken = default)
    {
        return (from dr in _carcassContext.ManyToManyJoins
            join usr in _carcassContext.Users on new { a = dr.PtId, b = dr.PKey } equals
                new { a = userDataId, b = usr.UserName }
            join rol in _carcassContext.Roles on new { a = dr.CtId, b = dr.CKey } equals
                new { a = roleDataId, b = rol.RolKey }
            group rol by usr.UsrId
            into ug
            select new Tuple<int, int>(ug.Key, ug.Min(usr => usr.RolLevel))).ToListAsync(cancellationToken);
    }

    public async Task<List<UserModel>> GetUsers(CancellationToken cancellationToken = default)
    {
        var users = await _carcassContext.Users.ToListAsync(cancellationToken);
        return users.Select(x => x.AdaptTo()).ToList();
    }

    public Task<List<ReturnValueModel>> GetRoleReturnValues(int minLevel, CancellationToken cancellationToken = default)
    {
        return _carcassContext.Roles.Where(w => w.RolLevel >= minLevel)
            .Select(role => new ReturnValueModel { Id = role.RolId, Key = role.RolKey, Name = role.RolName })
            .ToListAsync(cancellationToken);
    }

    public Task<List<DataTypeModelForRvs>> ParentsDataTypesNormalView(int dtDataId, string dataTypeKey, int userDataId,
        string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken = default)
    {
        var result = from dt in _carcassContext.DataTypes
            join pc in ManyToManyJoinsPc(dtDataId, dataTypeKey, dtDataId) on dt.DtKey equals pc
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey equals pcc2
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return result.ToListAsync(cancellationToken);
    }

    public Task<List<DataTypeModelForRvs>> ParentsDataTypesReverseView(int dtDataId, int userDataId, string userName,
        int roleDataId, int mmjDataId, CancellationToken cancellationToken = default)
    {
        var result = from dt in _carcassContext.DataTypes
            join dr in _carcassContext.ManyToManyJoins on new { a = dt.DtKey, b = dtDataId, c = dtDataId } equals
                new { a = dr.CKey, b = dr.PtId, c = dr.CtId }
            join pcc3 in ManyToManyJoinsPcc3(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey equals pcc3
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return result.ToListAsync(cancellationToken);
    }

    public Task<List<DataTypeModelForRvs>> ChildrenDataTypesNormalView(int dtDataId, string parentTypeKey,
        int userDataId, string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken = default)
    {
        var result = from dt in _carcassContext.DataTypes
            join pc in ManyToManyJoinsPc(dtDataId, parentTypeKey, dtDataId) on dt.DtKey equals pc
            join pcc3 in ManyToManyJoinsPcc3(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey equals pcc3
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return result.ToListAsync(cancellationToken);
    }

    public Task<List<DataTypeModelForRvs>> ChildrenDataTypesReverseView(int dtDataId, string parentTypeKey,
        int userDataId, string userName, int roleDataId, int mmjDataId, CancellationToken cancellationToken = default)
    {
        var result = from dt in _carcassContext.DataTypes
            join cp in ManyToManyJoinsCp(dtDataId, parentTypeKey, dtDataId) on dt.DtKey equals cp
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey equals pcc2
            select new DataTypeModelForRvs(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtIdFieldName,
                dt.DtKeyFieldName, dt.DtNameFieldName, dt.DtParentDataTypeId, dt.DtManyToManyJoinParentDataTypeId,
                dt.DtManyToManyJoinChildDataTypeId);
        return result.ToListAsync(cancellationToken);
    }

    public Task<List<TypeDataModel>> HalfChecksNormalView(int userDataId, string userName, int roleDataId,
        int mmjDataId, int dtDataId, int dataTypeId, string dataKey, CancellationToken cancellationToken = default)
    {
        var normViewResult = (from dr in _carcassContext.ManyToManyJoins
            join dt in _carcassContext.DataTypes on dr.CtId equals dt.DtId
            join pcc3 in ManyToManyJoinsPcc3(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey equals pcc3
            where dr.PtId == dataTypeId && dr.PKey == dataKey
            select new TypeDataModel(dr.CtId, dr.CKey)).Distinct();
        return normViewResult.ToListAsync(cancellationToken);
    }

    public Task<List<TypeDataModel>> HalfChecksReverseView(int userDataId, string userName, int roleDataId,
        int mmjDataId, int dtDataId, int dataTypeId, string dataKey, CancellationToken cancellationToken = default)
    {
        var result = (from dr in _carcassContext.ManyToManyJoins
            join dt in _carcassContext.DataTypes on dr.PtId equals dt.DtId
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey equals pcc2
            where dr.CtId == dataTypeId && dr.CKey == dataKey
            select new TypeDataModel(dr.PtId, dr.PKey)).Distinct();
        return result.ToListAsync(cancellationToken);
    }

    public Task<string?> DataTypeKeyById(int dtId, CancellationToken cancellationToken = default)
    {
        return _carcassContext.DataTypes.Where(w => w.DtId == dtId).Select(s => s.DtKey)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ManyToManyJoinModel?> GetOneManyToManyJoin(int parentDtId, string parentDKey, int childDtId,
        string childDKey, CancellationToken cancellationToken = default)
    {
        var mmj = await ManyToManyJoin(parentDtId, parentDKey, childDtId, childDKey, cancellationToken);
        return mmj?.AdaptTo();
    }

    public async Task<bool> RemoveOneManyToManyJoin(ManyToManyJoinModel manyToManyJoinModel,
        CancellationToken cancellationToken = default)
    {
        var mmj = await ManyToManyJoin(manyToManyJoinModel.PtId, manyToManyJoinModel.PKey, manyToManyJoinModel.CtId,
            manyToManyJoinModel.CKey, cancellationToken);
        if (mmj is null)
            return false;
        _carcassContext.ManyToManyJoins.Remove(mmj);
        return true;
    }

    public async Task<bool> CreateAndSaveOneManyToManyJoin(int parentDtId, string parentDKey, int childDtId,
        string childDKey, CancellationToken cancellationToken = default)
    {
        var parentDataType =
            await _carcassContext.DataTypes.SingleOrDefaultAsync(x => x.DtId == parentDtId, cancellationToken);
        if (parentDataType is null)
        {
            _logger.Log(LogLevel.Error, "parentDataType is null");
            return false;
        }

        var childDataType =
            await _carcassContext.DataTypes.SingleOrDefaultAsync(x => x.DtId == childDtId, cancellationToken);
        if (childDataType is null)
        {
            _logger.Log(LogLevel.Error, "childDataType is null");
            return false;
        }

        await _carcassContext.ManyToManyJoins.AddAsync(
            new ManyToManyJoin
            {
                ParentDataTypeNavigation = parentDataType,
                PtId = parentDataType.DtId,
                PKey = parentDKey,
                ChildDataTypeNavigation = childDataType,
                CtId = childDataType.DtId,
                CKey = childDKey
            }, cancellationToken);

        return true;
    }

    public IQueryable<string> ManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.PtId == parentTypeId && w.PKey == parentKey && w.CtId == childTypeId).Select(s => s.CKey);
        //.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<Tuple<string, string>>> ManyToManyJoinsPcc4(int parentTypeId, string parentKey,
        int childTypeId, int mmjDataId, int childTypeId2, int childTypeId3,
        CancellationToken cancellationToken = default)
    {
        var manyToManyJoins = await _carcassContext.ManyToManyJoins.ToListAsync(cancellationToken);
        return (from r in manyToManyJoins
            join r1 in manyToManyJoins on new { t = r.PtId, i = r.PKey } equals new { t = r1.CtId, i = r1.CKey }
            join drt in manyToManyJoins on r.CKey equals drt.PKey + "." + drt.CKey
            where r.CtId == mmjDataId && r.PtId == childTypeId && r1.PtId == parentTypeId && r1.PKey == parentKey &&
                  drt.PtId == childTypeId2 && drt.CtId == childTypeId3
            select new Tuple<string, string>(drt.PKey, drt.CKey)).ToList();
    }

    private Task<ManyToManyJoin?> ManyToManyJoin(int parentDtId, string parentDKey, int childDtId, string childDKey,
        CancellationToken cancellationToken = default)
    {
        return _carcassContext.ManyToManyJoins.SingleOrDefaultAsync(
            c => c.PtId == parentDtId && c.PKey == parentDKey && c.CtId == childDtId && c.CKey == childDKey,
            cancellationToken);
    }

    private IEnumerable<string> ManyToManyJoinsCp(int childTypeId, string childKey, int parentTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.CtId == childTypeId && w.CKey == childKey && w.PtId == parentTypeId).Select(s => s.PKey);
    }

    private IQueryable<string> ManyToManyJoinsPcc2(int parentTypeId, string parentKey, int childTypeId, int mmjDataId,
        int childTypeId2, int childTypeId3)
    {
        return from r in _carcassContext.ManyToManyJoins
            join r1 in _carcassContext.ManyToManyJoins on new { t = r.PtId, i = r.PKey } equals
                new { t = r1.CtId, i = r1.CKey }
            join drt in _carcassContext.ManyToManyJoins on r.CKey equals drt.PKey + "." + drt.CKey
            where r.CtId == mmjDataId && r.PtId == childTypeId && r1.PtId == parentTypeId && r1.PKey == parentKey &&
                  drt.PtId == childTypeId2 && drt.CtId == childTypeId3
            select drt.PKey;
    }

    private IEnumerable<string> ManyToManyJoinsPcc3(int parentTypeId, string parentKey, int childTypeId, int mmjDataId,
        int childTypeId2, int childTypeId3)
    {
        return from r in _carcassContext.ManyToManyJoins
            join r1 in _carcassContext.ManyToManyJoins on new { t = r.PtId, i = r.PKey } equals
                new { t = r1.CtId, i = r1.CKey }
            join drt in _carcassContext.ManyToManyJoins on r.CKey equals drt.PKey + "." + drt.CKey
            where r.CtId == mmjDataId && r.PtId == childTypeId && r1.PtId == parentTypeId && r1.PKey == parentKey &&
                  drt.PtId == childTypeId2 && drt.CtId == childTypeId3
            select drt.CKey;
    }
}