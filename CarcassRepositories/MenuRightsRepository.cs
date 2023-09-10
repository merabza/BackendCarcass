using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.V1.Responses;
using CarcassDb;
using CarcassDb.Models;
using CarcassDb.QueryModels;
using CarcassMasterDataDom;
using CarcassRepositories.Models;
using CarcassRightsDom;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

namespace CarcassRepositories;

public sealed class MenuRightsRepository : IMenuRightsRepository
{
    private readonly CarcassDbContext _carcassContext;

    //private readonly IDataTypeKeys _dataTypeKeys;
    private readonly ILogger<MenuRightsRepository> _logger;
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;

    public MenuRightsRepository(CarcassDbContext context, ILogger<MenuRightsRepository> logger,
        IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _carcassContext = context;
        _logger = logger;
        //_dataTypeKeys = dataTypeKeys;
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<MainMenuModel> MainMenu(string userName, CancellationToken cancellationToken)
    {
        var menuGroupsRes = await MenuGroups(userName, cancellationToken);
        ICollection<MenuGroupModel> menuGroups = await menuGroupsRes.ToListAsync(cancellationToken);
        var menuRes = await MenuItems(userName, cancellationToken);
        ICollection<MenuItmModel> menu = await menuRes.ToListAsync(cancellationToken);

        MainMenuModel mainMenuModel = new();
        foreach (var menuGroup in menuGroups)
        {
            menuGroup.Menu = menu.Where(m => m.MenGroupId == menuGroup.MengId).ToList();
            mainMenuModel.MenuGroups.Add(menuGroup);
        }

        return mainMenuModel;
    }

    public async Task<List<DataTypeModel>> ParentsTreeData(string userName, ERightsEditorViewStyle viewStyle,
        CancellationToken cancellationToken)
    {
        var dataTypes =
            (viewStyle == ERightsEditorViewStyle.NormalView
                ? await ParentsDataTypesNormalView(userName, cancellationToken)
                : await ParentsDataTypesReverseView(userName, cancellationToken)).OrderBy(o => o.DtName)
            .GroupBy(g => g.DtId)
            .Select(s => s.First());

        List<DataTypeModel> dataTypeModels = new();

        foreach (var dataType in dataTypes)
        {
            var entResult = await EntityForRetValues(dataType, userName, cancellationToken);

            if (entResult.IsT1)
                dataType.Errors.AddRange(entResult.AsT1);
            else
                dataType.ReturnValues = ReturnValues(entResult.AsT0);
            dataTypeModels.Add(dataType);
        }

        return dataTypeModels;
    }

    public async Task<List<DataTypeModel>> ChildrenTreeData(string userName, string dataTypeKey,
        ERightsEditorViewStyle viewStyle, CancellationToken cancellationToken)
    {
        var dataTypes =
            (viewStyle == ERightsEditorViewStyle.NormalView
                ? await ChildrenDataTypesNormalView(userName, dataTypeKey, cancellationToken)
                : await ChildrenDataTypesReverseView(userName, dataTypeKey, cancellationToken)).OrderBy(o => o.DtName)
            .GroupBy(g => g.DtId)
            .Select(s => s.First()).ToList();

        List<DataTypeModel> dataTypeModels = new();

        foreach (var dataType in dataTypes)
        {
            var entResult = await EntityForRetValues(dataType, userName, cancellationToken);

            if (entResult.IsT1)
                dataType.Errors.AddRange(entResult.AsT1);
            else
                dataType.ReturnValues = ReturnValues(entResult.AsT0);
            dataTypeModels.Add(dataType);
        }

        return dataTypeModels;
    }


    public async Task<Option<Err[]>> OptimizeRights(CancellationToken cancellationToken)
    {
        var errors = new List<Err>();
        var result = await ClearRights(ERightsSides.Parent, cancellationToken);
        if (result.IsSome)
            errors.AddRange((Err[])result);
        result = await ClearRights(ERightsSides.Child, cancellationToken);
        if (result.IsSome)
            errors.AddRange((Err[])result);
        if (errors.Count > 0)
            return errors.ToArray();
        await _carcassContext.SaveChangesAsync(cancellationToken);
        return null;
    }

    public async Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken)
    {
        var userDataTypeId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);
        var roleDataTypeId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var appClaimDataTypeId = await DataTypeIdByKey(ECarcassDataTypeKeys.AppClaim, cancellationToken);

        return await Task.FromResult(ManyToManyJoinsPcc(userDataTypeId, userName, roleDataTypeId, appClaimDataTypeId)
            .ToList());
    }

    public async Task<bool> SaveRightsChanges(string userName, List<RightsChangeModel> changedRights,
        CancellationToken cancellationToken)
    {
        try
        {
            var dtDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
            var mmjDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToDataType, cancellationToken);
            var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
            var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);
            var allowPairs =
                ManyToManyJoinsPcc4(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId).ToList();

            foreach (var drr in changedRights)
            {
                if (drr.Parent is null || drr.Child is null)
                    throw new Exception("SaveRightsChanges: parent or child keys are not valid");

                var parentKey = await DataTypeKeyById(drr.Parent.DtId, cancellationToken);
                var childKey = await DataTypeKeyById(drr.Child.DtId, cancellationToken);

                if (parentKey is null || childKey is null)
                    throw new Exception("SaveRightsChanges: parent or child keys are not valid");

                if (!allowPairs.Contains(new Tuple<string, string>(parentKey, childKey)))
                    continue;
                var mmj = _carcassContext.ManyToManyJoins.SingleOrDefault(c =>
                    c.PtId == drr.Parent.DtId && c.PKey == drr.Parent.DKey &&
                    c.CtId == drr.Child.DtId && c.CKey == drr.Child.DKey);
                if (mmj == null && drr.Checked)
                {
                    var parentDataType = _carcassContext.DataTypes.SingleOrDefault(x => x.DtId == drr.Parent.DtId);
                    if (parentDataType is null)
                    {
                        _logger.Log(LogLevel.Error, "parentDataType is null");
                        return false;
                    }

                    var childDataType = _carcassContext.DataTypes.SingleOrDefault(x => x.DtId == drr.Child.DtId);
                    if (childDataType is null)
                    {
                        _logger.Log(LogLevel.Error, "childDataType is null");
                        return false;
                    }

                    await _carcassContext.ManyToManyJoins.AddAsync(new ManyToManyJoin
                    {
                        ParentDataTypeNavigation = parentDataType,
                        PtId = parentDataType.DtId,
                        PKey = drr.Parent.DKey,
                        ChildDataTypeNavigation = childDataType,
                        CtId = childDataType.DtId,
                        CKey = drr.Child.DKey
                    }, cancellationToken);
                }
                else if (mmj != null && !drr.Checked)
                {
                    _carcassContext.ManyToManyJoins.Remove(mmj);
                }
            }

            await _carcassContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return false;
        }
    }

    public async Task<string?> GridModel(string tableName, CancellationToken cancellationToken)
    {
        var dataType =
            await _carcassContext.DataTypes.SingleOrDefaultAsync(s => s.DtTable == tableName, cancellationToken);
        return dataType?.DtGridRulesJson;
    }

    public async Task<DataTypesResponse[]> DataTypes(string userName, CancellationToken cancellationToken)
    {
        var dataTypeDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
        var roleDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);

        var qPccDt = ManyToManyJoinsPcc(userDtId, userName, roleDtId, dataTypeDtId);

        var dtIdsDist = (from dt in _carcassContext.DataTypes
            join pccDt in qPccDt on dt.DtKey equals pccDt
            select dt.DtId).Distinct().AsEnumerable();

        var res = (from dtf in dtIdsDist
                join dt in _carcassContext.DataTypes on dtf equals dt.DtId
                select new DataTypesResponse(dt.DtTable, dt.DtName, dt.DtNameNominative, dt.DtNameGenitive,
                    dt.DtIdFieldName, dt.DtKeyFieldName, dt.DtNameFieldName))
            .OrderBy(o => o.DtTable);

        var dtCrudRightsRes = await DataTypeCrudRights(userName, cancellationToken);
        ICollection<DataTypeToCrudRight> dtCrudRights = await dtCrudRightsRes.ToListAsync(cancellationToken);

        var res2 = res.ToArray();
        foreach (var dataType in res2)
        foreach (var dataTypeToCrudRight in dtCrudRights.Where(w => w.DtTable == dataType.DtTable))
            SetDataTypeCrudRight(dataTypeToCrudRight.CrtKey, dataType);

        return res2.ToArray();
    }

    public async Task<List<TypeDataModel>> HalfChecks(string userName, int dataTypeId, string dataKey,
        ERightsEditorViewStyle viewStyle, CancellationToken cancellationToken)
    {
        var dtDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
        var mmjDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToDataType, cancellationToken);
        var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);

        if (viewStyle == ERightsEditorViewStyle.NormalView)
        {
            var normViewResult = (from dr in _carcassContext.ManyToManyJoins
                join dt in _carcassContext.DataTypes on dr.CtId equals dt.DtId
                join pcc3 in ManyToManyJoinsPcc3(userDataId, userName, roleDataId, mmjDataId, dtDataId,
                        dtDataId) on
                    dt.DtKey
                    equals pcc3
                where dr.PtId == dataTypeId && dr.PKey == dataKey
                select new TypeDataModel(dr.CtId, dr.CKey)).Distinct();
            return await normViewResult.ToListAsync(cancellationToken);
        }

        var result = (from dr in _carcassContext.ManyToManyJoins
            join dt in _carcassContext.DataTypes on dr.PtId equals dt.DtId
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey
                equals pcc2
            where dr.CtId == dataTypeId && dr.CKey == dataKey
            select new TypeDataModel(dr.PtId, dr.PKey)).Distinct();
        return await result.ToListAsync(cancellationToken);
    }

    private async Task<OneOf<IEnumerable<IDataType>, Err[]>> EntitiesByTableName(string tableName,
        CancellationToken cancellationToken)
    {
        var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(tableName);
        return await loader.GetAllRecords(cancellationToken);
    }

    private async Task<OneOf<IEnumerable<IDataType>, Err[]>> EntityForRetValues(DataTypeModel dataType, string userName,
        CancellationToken cancellationToken)
    {
        if (dataType.DtKey == ECarcassDataTypeKeys.User.ToDtKey())
        {
            var minOfLevel = await UserMinLevel(userName, cancellationToken);
            var uml = await UsersMinLevels(cancellationToken);
            var users = await _carcassContext.Users.ToListAsync(cancellationToken);
            return (from usr in users
                join ml in uml on usr.UsrId equals ml.Item1 into gj
                from s in gj.DefaultIfEmpty()
                where (s?.Item2 ?? int.MaxValue) >= minOfLevel
                select usr).Cast<IDataType>().ToList();
        }

        if (dataType.DtKey == ECarcassDataTypeKeys.Role.ToDtKey())
        {
            var minLevel = await UserMinLevel(userName, cancellationToken);
            return _carcassContext.Roles.Where(w => w.RolLevel >= minLevel).Cast<IDataType>().ToList();
        }

        return await EntitiesByTableName(dataType.DtTable, cancellationToken);
    }


    private static List<ReturnValueModel> ReturnValues(IEnumerable<IDataType> ent)
    {
        return ent.Select(s => new ReturnValueModel { Value = s.Id, Key = s.Key, Name = s.Name, ParentId = s.ParentId })
            .ToList();
    }

    private async Task<List<Tuple<int, int>>> UsersMinLevels(CancellationToken cancellationToken)
    {
        var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);

        return (from dr in _carcassContext.ManyToManyJoins
            join usr in _carcassContext.Users on new { a = dr.PtId, b = dr.PKey } equals new
                { a = userDataId, b = usr.UserName }
            join rol in _carcassContext.Roles on new { a = dr.CtId, b = dr.CKey } equals new
                { a = roleDataId, b = rol.RolKey }
            group rol by usr.UsrId
            into ug
            select new Tuple<int, int>(ug.Key, ug.Min(usr => usr.RolLevel))).ToList();
    }

    private async Task<int> UserMinLevel(string userName, CancellationToken cancellationToken)
    {
        var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);

        var drPcs = ManyToManyJoinsPc(userDataId, userName, roleDataId).ToList();

        return (from drPc in drPcs
            join rol in _carcassContext.Roles on drPc equals rol.RolKey
            select rol.RolLevel).DefaultIfEmpty(1000).Min();
    }


    private async Task<List<DataTypeModel>> ParentsDataTypesNormalView(string userName,
        CancellationToken cancellationToken)
    {
        var dtDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
        var mmjDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToDataType, cancellationToken);
        var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);

        var dataTypeKey = ECarcassDataTypeKeys.DataType.ToDtKey();

        var result = from dt in _carcassContext.DataTypes
            join pc in ManyToManyJoinsPc(dtDataId, dataTypeKey, dtDataId) on dt.DtKey equals pc
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey
                equals
                pcc2
            select new DataTypeModel(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtParentDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    private async Task<List<DataTypeModel>> ParentsDataTypesReverseView(string userName,
        CancellationToken cancellationToken)
    {
        var dtDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
        var mmjDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToDataType, cancellationToken);
        var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);


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
            select new DataTypeModel(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtParentDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    private async Task<List<DataTypeModel>> ChildrenDataTypesNormalView(string userName, string parentTypeKey,
        CancellationToken cancellationToken)
    {
        var dtDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
        var mmjDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToDataType, cancellationToken);
        var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);


        var result = from dt in _carcassContext.DataTypes
            join pc in ManyToManyJoinsPc(dtDataId, parentTypeKey, dtDataId) on dt.DtKey equals pc
            join pcc3 in ManyToManyJoinsPcc3(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey
                equals
                pcc3
            select new DataTypeModel(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtParentDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    private async Task<List<DataTypeModel>> ChildrenDataTypesReverseView(string userName, string parentTypeKey,
        CancellationToken cancellationToken)
    {
        var dtDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataType, cancellationToken);
        var mmjDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToDataType, cancellationToken);
        var roleDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDataId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);


        var result = from dt in _carcassContext.DataTypes
            join cp in ManyToManyJoinsCp(dtDataId, parentTypeKey, dtDataId) on dt.DtKey equals cp
            join pcc2 in ManyToManyJoinsPcc2(userDataId, userName, roleDataId, mmjDataId, dtDataId, dtDataId) on
                dt.DtKey
                equals
                pcc2
            select new DataTypeModel(dt.DtId, dt.DtKey, dt.DtName, dt.DtTable, dt.DtParentDataTypeId);
        return await result.ToListAsync(cancellationToken);
    }

    private async Task<IQueryable<MenuGroupModel>> MenuGroups(string userName, CancellationToken cancellationToken)
    {
        var menuGroupsDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.MenuGroup, cancellationToken);
        var menuDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.MenuItm, cancellationToken);
        var roleDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);


        var mengIdsDist = (from m in _carcassContext.Menu
            join mg in _carcassContext.MenuGroups on m.MenGroupId equals mg.MengId
            //join f in CarcassContext.Forms on m.MenFormId equals f.FrmId
            join pccMg in ManyToManyJoinsPcc(userDtId, userName, roleDtId, menuGroupsDtId) on mg.MengKey equals pccMg
            join pccM in ManyToManyJoinsPcc(userDtId, userName, roleDtId, menuDtId) on m.MenKey equals pccM
            select mg.MengId).Distinct();


        return from mgf in mengIdsDist
            join mg in _carcassContext.MenuGroups on mgf equals mg.MengId
            orderby mg.SortId
            select new MenuGroupModel(mg.MengId, mg.MengKey, mg.MengName, mg.SortId, mg.MengIconName, mg.Hidden);
    }

    private async Task<IQueryable<MenuItmModel>> MenuItems(string userName, CancellationToken cancellationToken)
    {
        var menuGroupsDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.MenuGroup, cancellationToken);
        var menuDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.MenuItm, cancellationToken);
        var roleDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);


        return from m in _carcassContext.Menu
            join mg in _carcassContext.MenuGroups on m.MenGroupId equals mg.MengId
            //join f in CarcassContext.Forms on m.MenFormId equals f.FrmId
            join pccMg in ManyToManyJoinsPcc(userDtId, userName, roleDtId, menuGroupsDtId) on mg.MengKey equals pccMg
            join pccM in ManyToManyJoinsPcc(userDtId, userName, roleDtId, menuDtId) on m.MenKey equals pccM
            orderby mg.SortId, m.SortId
            select new MenuItmModel(m.MenId, m.MenKey, m.MenLinkKey, m.MenName, m.MenValue, m.MenGroupId, m.SortId,
                m.MenIconName);
    }

    private async Task<Option<Err[]>> ClearRights(ERightsSides rightSide, CancellationToken cancellationToken)
    {
        var rightsTable = DataTypesTableForRightsOptimization(rightSide);
        var errors = new List<Err>();
        foreach (var dataTypeTable in rightsTable)
        {
            var res = await DeleteUnusedRights(dataTypeTable.DtTable, rightSide, dataTypeTable.DtId, cancellationToken);
            if (res.IsSome)
                errors.AddRange((Err[])res);
        }

        return errors.Count > 0 ? errors.ToArray() : null;
    }

    private IEnumerable<DataTypeTableModel> DataTypesTableForRightsOptimization(ERightsSides rightSide)
    {
        return (from dr in _carcassContext.ManyToManyJoins
            join dt in _carcassContext.DataTypes on rightSide == ERightsSides.Parent ? dr.PtId : dr.CtId equals
                dt.DtId
            select new DataTypeTableModel(dt.DtId, dt.DtTable)).Distinct();
    }


    private async Task<Option<Err[]>> DeleteUnusedRights(string dtTable, ERightsSides rightSide, int dtId,
        CancellationToken cancellationToken)
    {
        var entResult = await EntitiesByTableName(dtTable, cancellationToken);
        if (entResult.IsT1)
            return entResult.AsT1;

        var ens = entResult.AsT0.ToList();
        var mmjs = _carcassContext.ManyToManyJoins.ToList();

        var forDelete = from dr in mmjs
            join t in ens on rightSide == ERightsSides.Parent ? dr.PKey : dr.CKey equals t.Key into drt
            from sub in drt.DefaultIfEmpty()
            where sub == null && (rightSide == ERightsSides.Parent ? dr.PtId : dr.CtId) == dtId
            select dr;

        _carcassContext.RemoveRange(forDelete);
        return null;
    }

    private static void SetDataTypeCrudRight(string crtKey, DataTypesResponse dataType)
    {
        if (!Enum.TryParse(crtKey, out ECrudOperationType crudType)) return;
        switch (crudType)
        {
            case ECrudOperationType.Create:
                dataType.Create = true;
                break;
            case ECrudOperationType.Update:
                dataType.Update = true;
                break;
            case ECrudOperationType.Delete:
                dataType.Delete = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<IQueryable<DataTypeToCrudRight>> DataTypeCrudRights(string userName,
        CancellationToken cancellationToken)
    {
        var userDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);
        var roleDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var dataTypeToCrudTypeDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.DataTypeToCrudType, cancellationToken);


        return from dt in _carcassContext.DataTypes
            from crt in _carcassContext.CrudRightTypes
            join pcc in ManyToManyJoinsPcc(userDtId, userName, roleDtId, dataTypeToCrudTypeDtId) on
                dt.DtKey + '.' + crt.CrtKey equals pcc
            select new DataTypeToCrudRight(dt.DtId, dt.DtTable, crt.CrtKey);
    }

    private async Task<int> DataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken)
    {
        return await _carcassContext.DataTypes.Where(w => w.DtKey == dataTypeKey.ToDtKey()).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<string?> DataTypeKeyById(int dtId, CancellationToken cancellationToken)
    {
        return await _carcassContext.DataTypes.Where(w => w.DtId == dtId).Select(s => s.DtKey)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IEnumerable<string> ManyToManyJoinsPc(int parentTypeId, string parentKey, int childTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.PtId == parentTypeId && w.PKey == parentKey && w.CtId == childTypeId).Select(s => s.CKey);
    }

    private IEnumerable<string> ManyToManyJoinsCp(int childTypeId, string childKey, int parentTypeId)
    {
        return _carcassContext.ManyToManyJoins
            .Where(w => w.CtId == childTypeId && w.CKey == childKey && w.PtId == parentTypeId).Select(s => s.PKey);
    }

    private IEnumerable<string> ManyToManyJoinsPcc(int parentTypeId, string parentKey, int childTypeId,
        int childTypeId2)
    {
        return from r1 in _carcassContext.ManyToManyJoins
            join r2 in _carcassContext.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
                { t = r2.PtId, i = r2.PKey }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId && r2.CtId == childTypeId2
            select r2.CKey;
    }

    private IEnumerable<string> ManyToManyJoinsPcc2(int parentTypeId, string parentKey, int childTypeId,
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

    private IEnumerable<string> ManyToManyJoinsPcc3(int parentTypeId, string parentKey, int childTypeId,
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

    private IEnumerable<Tuple<string, string>> ManyToManyJoinsPcc4(int parentTypeId, string parentKey,
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
}