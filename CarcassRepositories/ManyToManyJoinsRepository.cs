using System;
using System.Linq;
using CarcassDb;
using CarcassMasterDataDom;

namespace CarcassRepositories;

public /*open*/ class ManyToManyJoinsRepository : IManyToManyJoinsRepository
{
    private readonly CarcassDbContext _carcassContext;
    //private readonly IDataTypeKeys _dataTypeKeys;

    public ManyToManyJoinsRepository(CarcassDbContext ctx)
    {
        _carcassContext = ctx;
        //_dataTypeKeys = dataTypeKeys;
    }

    public bool CheckUserToUserRight(string userName1, string userName2)
    {
        var roleDataTypeId = GetDataTypeIdByKey(ECarcassDataTypeKeys.Role);
        var userDataTypeId = GetDataTypeIdByKey(ECarcassDataTypeKeys.User);

        return GetManyToManyJoinsPccpOne(userDataTypeId, userName1, roleDataTypeId, roleDataTypeId, userDataTypeId,
            userName2);
    }

    public bool CheckUserAppClaimRight(string userName, string appClaimName)
    {
        var roleDataTypeId = GetDataTypeIdByKey(ECarcassDataTypeKeys.Role);
        var userDataTypeId = GetDataTypeIdByKey(ECarcassDataTypeKeys.User);
        var appClaimDataTypeId = GetDataTypeIdByKey(ECarcassDataTypeKeys.AppClaim);

        return GetManyToManyJoinsPccOne(userDataTypeId, userName, roleDataTypeId, appClaimDataTypeId, appClaimName);
    }

    public int GetDataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey)
    {
        return _carcassContext.DataTypes.Where(w => w.DtKey == dataTypeKey.ToDtKey()).Select(s => s.DtId)
            .SingleOrDefault();
    }

    public string? GetDataTypeKeyById(int dtId)
    {
        return _carcassContext.DataTypes.Where(w => w.DtId == dtId).Select(s => s.DtKey).SingleOrDefault();
    }

    public string? GetKeyByTableName(string tableName)
    {
        return _carcassContext.DataTypes.Where(w => w.DtTable == tableName).Select(s => s.DtKey).SingleOrDefault();
    }

    public IQueryable<string> GetManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.PtId == parentTypeId && w.PKey == parentKey && w.CtId == childTypeId).Select(s => s.CKey);
    }

    public IQueryable<string> GetManyToManyJoinsCp(int childTypeId, string childKey, int parentTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.CtId == childTypeId && w.CKey == childKey && w.PtId == parentTypeId).Select(s => s.PKey);
    }

    public IQueryable<string> GetManyToManyJoinsPcc(int parentTypeId, string parentKey, int childTypeId,
        int childTypeId2)
    {
        //return from r in CarcassContext.ManyToManyJoins
        //  join r1 in CarcassContext.ManyToManyJoins on new {t = r.PtId, i = r.PKey} equals new {t = r1.CtId, i = r1.CKey}
        //  where r.CtId == childTypeId2 && r.PtId == childTypeId && r1.PtId == parentTypeId &&
        //        r1.PKey == parentKey
        //  select r.CKey;
        return from r1 in _carcassContext.ManyToManyJoins
            join r2 in _carcassContext.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
                { t = r2.PtId, i = r2.PKey }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId && r2.CtId == childTypeId2
            select r2.CKey;
    }

    public IQueryable<string> GetManyToManyJoinsPccc(int parentTypeId, string parentKey, int childTypeId,
        int childTypeId2, int childTypeId3)
    {
        return from r1 in _carcassContext.ManyToManyJoins
            join r2 in _carcassContext.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
                { t = r2.PtId, i = r2.PKey }
            join r3 in _carcassContext.ManyToManyJoins on new { t = r2.CtId, i = r2.CKey } equals new
                { t = r3.PtId, i = r3.PKey }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId &&
                  r2.CtId == childTypeId2 &&
                  r3.CtId == childTypeId3
            select r3.CKey;
    }

    public IQueryable<string> GetManyToManyJoinsPcc2(int parentTypeId, string parentKey, int childTypeId,
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

    public IQueryable<string> GetManyToManyJoinsPcc3(int parentTypeId, string parentKey, int childTypeId,
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

    public IQueryable<Tuple<string, string>> GetManyToManyJoinsPcc4(int parentTypeId, string parentKey,
        int childTypeId, int mmjDataId, int childTypeId2, int childTypeId3)
    {
        return from r in _carcassContext.ManyToManyJoins
            join r1 in _carcassContext.ManyToManyJoins on new { t = r.PtId, i = r.PKey } equals new
                { t = r1.CtId, i = r1.CKey }
            join drt in _carcassContext.ManyToManyJoins on r.CKey equals drt.PKey + "." + drt.CKey
            where r.CtId == mmjDataId && r.PtId == childTypeId && r1.PtId == parentTypeId &&
                  r1.PKey == parentKey && drt.PtId == childTypeId2 && drt.CtId == childTypeId3
            select new Tuple<string, string>(drt.PKey, drt.CKey);
    }

    private bool GetManyToManyJoinsPccOne(int parentTypeId, string parentKey, int childTypeId,
        int childTypeId2, string childKey2)
    {
        //return from r in CarcassContext.ManyToManyJoins
        //  join r1 in CarcassContext.ManyToManyJoins on new {t = r.PtId, i = r.PKey} equals new {t = r1.CtId, i = r1.CKey}
        //  where r.CtId == childTypeId2 && r.CKey == childKey2 && r.PtId == childTypeId && r1.PtId == parentTypeId &&
        //        r1.PKey == parentKey
        //  select r.CKey;
        return (from r1 in _carcassContext.ManyToManyJoins
            join r2 in _carcassContext.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
                { t = r2.PtId, i = r2.PKey }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId &&
                  r2.CtId == childTypeId2 &&
                  r2.CKey == childKey2
            select r2.CKey).Any();
    }

    //მრავალი მრავალთან კავშირების ვარიანტი: მშობელი -> შვილი -> შვილი -> მშობელი
    //მაგალითად თუ გვაქვს 2 მომხმარებელი user1 და user2 გვაინრერესებს მათი როლების კავშირი,
    //ანუ user1-ის ერთ-ერთ როლს აქვს თუ არა უფლება user2-ის რომელიმე როლზე
    private bool GetManyToManyJoinsPccpOne(int parentTypeId, string parentKey, int childTypeId, int childTypeId2,
        int parentTypeId3, string parentKey3)
    {
        return (from r1 in _carcassContext.ManyToManyJoins
            join r2 in _carcassContext.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
                { t = r2.PtId, i = r2.PKey }
            join r3 in _carcassContext.ManyToManyJoins on new { t = r2.CtId, i = r2.CKey } equals new
                { t = r3.CtId, i = r3.CKey }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId &&
                  r2.CtId == childTypeId2 &&
                  r3.PtId == parentTypeId3 && r3.PKey == parentKey3
            select r3.MmjId).Any();
    }
}