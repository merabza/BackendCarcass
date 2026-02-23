using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Database;
using BackendCarcass.Database.Models;
using BackendCarcass.Database.QueryModels;
using BackendCarcass.Repositories.Models;
using BackendCarcass.Rights;
using BackendCarcassContracts.V1.Responses;
using Microsoft.EntityFrameworkCore;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.Repositories;

public sealed class MenuRightsRepository : IMenuRightsRepository
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly CarcassDbContext _carcassContext;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuRightsRepository(CarcassDbContext context, IUnitOfWork unitOfWork)
    {
        _carcassContext = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<MainMenuModel> MainMenu(string userName, CancellationToken cancellationToken = default)
    {
        IQueryable<MenuGroupModel> menuGroupsRes = await MenuGroups(userName, cancellationToken);
        ICollection<MenuGroupModel> menuGroups = await menuGroupsRes.ToListAsync(cancellationToken);
        IQueryable<MenuItmModel> menuRes = await MenuItems(userName, cancellationToken);
        ICollection<MenuItmModel> menu = await menuRes.ToListAsync(cancellationToken);

        var mainMenuModel = new MainMenuModel();
        foreach (MenuGroupModel menuGroup in menuGroups)
        {
            menuGroup.Menu = menu.Where(m => m.MenGroupId == menuGroup.MengId).ToList();
            mainMenuModel.MenuGroups.Add(menuGroup);
        }

        return mainMenuModel;
    }

    //public async Task<List<string>> UserAppClaims(string userName, CancellationToken cancellationToken = default)
    //{
    //    var userDataTypeId = await DataTypeIdByKey(GetTableName<User>(), cancellationToken);
    //    var roleDataTypeId = await DataTypeIdByKey(GetTableName<Role>(), cancellationToken);
    //    var appClaimDataTypeId = await DataTypeIdByKey(GetTableName<AppClaim>(), cancellationToken);

    //    return await Task.FromResult(ManyToManyJoinsPcc(userDataTypeId, userName, roleDataTypeId, appClaimDataTypeId)
    //        .ToList());
    //}

    public async Task<string?> GridModel(string tableName, CancellationToken cancellationToken = default)
    {
        DataType? dataType =
            await _carcassContext.DataTypes.SingleOrDefaultAsync(s => s.DtTable == tableName, cancellationToken);
        return dataType?.DtGridRulesJson;
    }

    public async Task<DataTypesResponse[]> DataTypes(string userName, CancellationToken cancellationToken = default)
    {
        int dataTypeDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<DataType>(), cancellationToken);
        int roleDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        int userDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<User>(), cancellationToken);

        IEnumerable<string> qPccDt = ManyToManyJoinsPcc(userDtId, userName, roleDtId, dataTypeDtId);

        IEnumerable<int> dtIdsDist = (from dt in _carcassContext.DataTypes
            join pccDt in qPccDt on dt.DtTable equals pccDt
            select dt.DtId).Distinct().AsEnumerable();

        IOrderedEnumerable<DataTypesResponse> res = (from dtf in dtIdsDist
            join dt in _carcassContext.DataTypes on dtf equals dt.DtId
            select new DataTypesResponse(dt.DtTable, dt.DtName, dt.DtNameNominative, dt.DtNameGenitive,
                dt.DtIdFieldName, dt.DtKeyFieldName, dt.DtNameFieldName)).OrderBy(o => o.DtTable);

        IQueryable<DataTypeToCrudRight> dtCrudRightsRes = await DataTypeCrudRights(userName, cancellationToken);
        ICollection<DataTypeToCrudRight> dtCrudRights = await dtCrudRightsRes.ToListAsync(cancellationToken);

        DataTypesResponse[] res2 = res.ToArray();
        foreach (DataTypesResponse dataType in res2)
        {
            foreach (DataTypeToCrudRight dataTypeToCrudRight in dtCrudRights.Where(w => w.DtTable == dataType.DtTable))
            {
                SetDataTypeCrudRight(dataTypeToCrudRight.CrtKey, dataType);
            }
        }

        return [.. res2];
    }

    private async Task<IQueryable<MenuGroupModel>> MenuGroups(string userName,
        CancellationToken cancellationToken = default)
    {
        int menuGroupsDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<MenuGroup>(), cancellationToken);
        int menuDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<MenuItm>(), cancellationToken);
        int roleDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        int userDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<User>(), cancellationToken);

        IQueryable<int>? mngIdsDist = (from m in _carcassContext.Menu
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

    private async Task<IQueryable<MenuItmModel>> MenuItems(string userName,
        CancellationToken cancellationToken = default)
    {
        int menuGroupsDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<MenuGroup>(), cancellationToken);
        int menuDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<MenuItm>(), cancellationToken);
        int roleDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        int userDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<User>(), cancellationToken);

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
        if (!Enum.TryParse(crtKey, out ECrudOperationType crudType))
        {
            return;
        }

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
                throw new ArgumentOutOfRangeException(nameof(crtKey), crtKey,
                    $"Unexpected CRUD operation type: {crtKey}");
        }
    }

    private async Task<IQueryable<DataTypeToCrudRight>> DataTypeCrudRights(string userName,
        CancellationToken cancellationToken = default)
    {
        int userDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<User>(), cancellationToken);
        int roleDtId = await DataTypeIdByKey(_unitOfWork.GetTableName<Role>(), cancellationToken);
        int dataTypeToCrudTypeDtId =
            await DataTypeIdByKey($"{_unitOfWork.GetTableName<DataType>()}{_unitOfWork.GetTableName<CrudRightType>()}",
                cancellationToken);

        return from dt in _carcassContext.DataTypes
            from crt in _carcassContext.CrudRightTypes
            join pcc in ManyToManyJoinsPcc(userDtId, userName, roleDtId, dataTypeToCrudTypeDtId) on
                dt.DtTable + '.' + crt.CrtKey equals pcc
            select new DataTypeToCrudRight(dt.DtId, dt.DtTable, crt.CrtKey);
    }

    private Task<int> DataTypeIdByKey(string tableName, CancellationToken cancellationToken = default)
    {
        return _carcassContext.DataTypes.Where(w => w.DtTable == tableName).Select(s => s.DtId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IEnumerable<string> ManyToManyJoinsPcc(int parentTypeId, string parentKey, int childTypeId,
        int childTypeId2)
    {
        return from r1 in _carcassContext.ManyToManyJoins
            join r2 in _carcassContext.ManyToManyJoins on new { t = r1.CtId, i = r1.CKey } equals new
            {
                t = r2.PtId, i = r2.PKey
            }
            where r1.PtId == parentTypeId && r1.PKey == parentKey && r1.CtId == childTypeId && r2.CtId == childTypeId2
            select r2.CKey;
    }
}
