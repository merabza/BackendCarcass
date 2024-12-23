using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.V1.Responses;
using CarcassDb;
using CarcassDb.QueryModels;
using CarcassDom;
using CarcassMasterDataDom;
using CarcassRepositories.Models;
using Microsoft.EntityFrameworkCore;

namespace CarcassRepositories;

public sealed class MenuRightsRepository(CarcassDbContext context) : IMenuRightsRepository
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly CarcassDbContext _carcassContext = context;

    public async Task<MainMenuModel> MainMenu(string userName, CancellationToken cancellationToken = default)
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

    public async Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default)
    {
        var userDataTypeId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);
        var roleDataTypeId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var appClaimDataTypeId = await DataTypeIdByKey(ECarcassDataTypeKeys.AppClaim, cancellationToken);

        return await Task.FromResult(ManyToManyJoinsPcc(userDataTypeId, userName, roleDataTypeId, appClaimDataTypeId)
            .ToList());
    }

    public async Task<string?> GridModel(string tableName, CancellationToken cancellationToken = default)
    {
        var dataType =
            await _carcassContext.DataTypes.SingleOrDefaultAsync(s => s.DtTable == tableName, cancellationToken);
        return dataType?.DtGridRulesJson;
    }

    public async Task<DataTypesResponse[]> DataTypes(string userName, CancellationToken cancellationToken = default)
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

        return [.. res2];
    }

    private async Task<IQueryable<MenuGroupModel>> MenuGroups(string userName, CancellationToken cancellationToken = default)
    {
        var menuGroupsDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.MenuGroup, cancellationToken);
        var menuDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.MenuItm, cancellationToken);
        var roleDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.Role, cancellationToken);
        var userDtId = await DataTypeIdByKey(ECarcassDataTypeKeys.User, cancellationToken);


        var mngIdsDist = (from m in _carcassContext.Menu
            join mg in _carcassContext.MenuGroups on m.MenGroupId equals mg.MengId
            //join f in CarcassContext.Forms on m.MenFormId equals f.FrmId
            join pccMg in ManyToManyJoinsPcc(userDtId, userName, roleDtId, menuGroupsDtId) on mg.MengKey equals pccMg
            join pccM in ManyToManyJoinsPcc(userDtId, userName, roleDtId, menuDtId) on m.MenKey equals pccM
            select mg.MengId).Distinct();


        return from mgf in mngIdsDist
            join mg in _carcassContext.MenuGroups on mgf equals mg.MengId
            orderby mg.SortId
            select new MenuGroupModel(mg.MengId, mg.MengKey, mg.MengName, mg.SortId, mg.MengIconName, mg.Hidden);
    }

    private async Task<IQueryable<MenuItmModel>> MenuItems(string userName, CancellationToken cancellationToken = default)
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
        CancellationToken cancellationToken = default)
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

    private async Task<int> DataTypeIdByKey(ECarcassDataTypeKeys dataTypeKey, CancellationToken cancellationToken = default)
    {
        return await _carcassContext.DataTypes.Where(w => w.DtKey == dataTypeKey.ToDtKey()).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
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
}