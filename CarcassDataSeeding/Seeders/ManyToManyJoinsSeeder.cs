using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Comparers;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom;
using CarcassMasterDataDom.CellModels;
using DatabaseToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class ManyToManyJoinsSeeder : DataSeeder<ManyToManyJoin, ManyToManyJoinSeederModel>
{
    private readonly ICarcassDataSeederRepository _carcassRepo;
    private readonly string _secretDataFolder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ManyToManyJoinsSeeder(string secretDataFolder, ICarcassDataSeederRepository carcassRepo,
        string dataSeedFolder, IDataSeederRepository repo, ESeedDataType seedDataType = ESeedDataType.OnlyJson,
        List<string>? keyFieldNamesList = null) : base(dataSeedFolder, repo, seedDataType, keyFieldNamesList)
    {
        _secretDataFolder = secretDataFolder;
        _carcassRepo = carcassRepo;
    }

    public override bool AdditionalCheck(List<ManyToManyJoinSeederModel> jsonData, List<ManyToManyJoin> savedData)
    {
        //var dataTypeDKey = ECarcassDataTypeKeys.DataType.ToDtKey();
        //return Check(CreateMustListByRules()) && Check(GetThirdPartRights(
        //                                          ECarcassDataTypeKeys.DataTypeToDataType.ToDtKey(), dataTypeDKey,
        //                                          dataTypeDKey)) //rol admin mmj(dt,dt) all
        //                                      && Check(GetThirdPartRights(
        //                                          ECarcassDataTypeKeys.DataTypeToCrudType.ToDtKey(), dataTypeDKey,
        //                                          ECarcassDataTypeKeys.CrudRightType
        //                                              .ToDtKey())) //rol admin DataCrudRights all
        //                                      //&& Check(GetThirdPartRights(MenuToCrudTypeModel.DKey, MenuItm.DKey, CrudRightType.DKey)) //rol admin MenuCrudRights all
        //                                      && Check(GetAdminRoleToDataTypes()) //rol admin DataTypes all
        //                                      && Check(GetAdminRoleToMenuGroups()) //rol admin MenuGroups all
        //                                      && Check(GetAdminRoleToMenuItems()) //rol admin MenuItems all
        //                                      && Check(GetMenuToDataTypes()) //men -> DataTypes
        //                                      && Repo.RemoveNeedlessRecords(GetMenuToDataTypesNeedLess());

        var manyToManyJoinsList = new List<ManyToManyJoin>();
        var dataTypeTableName = _carcassRepo.GetTableName<DataType>();
        var crudRightTypeTableName = _carcassRepo.GetTableName<CrudRightType>();
        manyToManyJoinsList.AddRange(GetThirdPartRights($"{dataTypeTableName}{dataTypeTableName}", dataTypeTableName,
            dataTypeTableName));

        manyToManyJoinsList.AddRange(GetThirdPartRights($"{dataTypeTableName}{crudRightTypeTableName}",
            dataTypeTableName, crudRightTypeTableName));

        manyToManyJoinsList.AddRange(GetAdminRoleToDataTypes());
        manyToManyJoinsList.AddRange(GetAdminRoleToMenuGroups());
        manyToManyJoinsList.AddRange(GetAdminRoleToMenuItems());
        manyToManyJoinsList.AddRange(GetMenuToDataTypes());

        var existingManyToManyJoins = DataSeederRepo.GetAll<ManyToManyJoin>();

        if (!DataSeederRepo.CreateEntities(manyToManyJoinsList
                .Except(existingManyToManyJoins, new ManyToManyJoinComparer()).Distinct(new ManyToManyJoinComparer())
                .ToList()))
            throw new Exception("manyToManyJoinsList entities cannot be created");

        return DataSeederRepo.RemoveNeedlessRecords(GetMenuToDataTypesNeedLess());
    }

    //private bool Check(List<ManyToManyJoin> mustBeDataTypes)
    //{
    //    if (mustBeDataTypes.Count == 0)
    //        return true;
    //    var existingManyToManyJoins = Repo.GetAll<ManyToManyJoin>()
    //        .Select(s => new ManyToManyJoinDomain(s.PtId, s.PKey, s.CtId, s.CKey)).ToList();
    //    var forAdd = mustBeDataTypes.Select(s => new ManyToManyJoinDomain(s.PtId, s.PKey, s.CtId, s.CKey))
    //        .Except(existingManyToManyJoins).ToList();
    //    return Repo.CreateEntities(forAdd.Select(s => new ManyToManyJoin
    //    {
    //        PtId = s.PtId, PKey = s.PKey, CtId = s.CtId, CKey = s.CKey
    //    }).ToList());
    //}

    public override List<ManyToManyJoin> Adapt(List<ManyToManyJoinSeederModel> manyToManyJoinsSeedData)
    {
        var tempData = DataSeederTempData.Instance;
        return manyToManyJoinsSeedData.Select(s => new ManyToManyJoin
        {
            PtId = tempData.GetIntIdByKey<DataType>(s.PtIdDtKey),
            PKey = s.PKey,
            CtId = tempData.GetIntIdByKey<DataType>(s.CtIdDtKey),
            CKey = s.CKey
        }).ToList();
    }

    public override List<ManyToManyJoin> CreateListByRules()
    {
        return CreateMustListByRules().Distinct(new ManyToManyJoinComparer()).ToList();
    }

    protected virtual List<ManyToManyJoin> CreateMustListByRules()
    {
        var dataTypeTableName = _carcassRepo.GetTableName<DataType>();
        var userTableName = _carcassRepo.GetTableName<User>();
        var roleTableName = _carcassRepo.GetTableName<Role>();
        var menuGroupTableName = _carcassRepo.GetTableName<MenuGroup>();
        var menuTableName = _carcassRepo.GetTableName<MenuItm>();
        var crudRightTypeTableName = _carcassRepo.GetTableName<CrudRightType>();

        var tempData = DataSeederTempData.Instance;
        var dataTypeDataTypeId = tempData.GetIntIdByKey<DataType>(dataTypeTableName);
        var userDataTypeId = tempData.GetIntIdByKey<DataType>(userTableName);
        var roleDataTypeId = tempData.GetIntIdByKey<DataType>(roleTableName);
        var menuGroupDataTypeId = tempData.GetIntIdByKey<DataType>(menuGroupTableName);
        var menuDataTypeId = tempData.GetIntIdByKey<DataType>(menuTableName);
        var crudRightTypeDataTypeId = tempData.GetIntIdByKey<DataType>(crudRightTypeTableName);

        const string adminRoleKey = "Admin";

        ManyToManyJoin[] newManyToManyJoins =
        [
            //dt dt dt [dt, usr, role, menu, crudRightType]
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = dataTypeTableName,
                CtId = dataTypeDataTypeId,
                CKey = dataTypeTableName
            },
            new()
            {
                PtId = dataTypeDataTypeId, PKey = dataTypeTableName, CtId = dataTypeDataTypeId, CKey = userTableName
            },
            new()
            {
                PtId = dataTypeDataTypeId, PKey = dataTypeTableName, CtId = dataTypeDataTypeId, CKey = roleTableName
            },
            new()
            {
                PtId = dataTypeDataTypeId, PKey = dataTypeTableName, CtId = dataTypeDataTypeId, CKey = menuTableName
            },
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = dataTypeTableName,
                CtId = dataTypeDataTypeId,
                CKey = crudRightTypeTableName
            },
            //dt mnu dt [dt, crudRightType]
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = menuTableName,
                CtId = dataTypeDataTypeId,
                CKey = crudRightTypeTableName
            },
            //dt usr dt rol
            new() { PtId = dataTypeDataTypeId, PKey = userTableName, CtId = dataTypeDataTypeId, CKey = roleTableName },
            //dt rol dt [meng, men, dt, DataTypeToDataTypeModel, DataTypeToCrudTypeModel, rol]
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = roleTableName,
                CtId = dataTypeDataTypeId,
                CKey = menuGroupTableName
            },
            new() { PtId = dataTypeDataTypeId, PKey = roleTableName, CtId = dataTypeDataTypeId, CKey = menuTableName },
            new()
            {
                PtId = dataTypeDataTypeId, PKey = roleTableName, CtId = dataTypeDataTypeId, CKey = dataTypeTableName
            },
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = roleTableName,
                CtId = dataTypeDataTypeId,
                CKey = $"{dataTypeTableName}{dataTypeTableName}"
            },
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = roleTableName,
                CtId = dataTypeDataTypeId,
                CKey = $"{dataTypeTableName}{crudRightTypeTableName}"
            },
            new() { PtId = dataTypeDataTypeId, PKey = roleTableName, CtId = dataTypeDataTypeId, CKey = roleTableName },
            //admin dt dt [dt, usr, rol]
            new() { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = dataTypeTableName },
            new() { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = userTableName },
            new() { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = roleTableName }
        ];
        var lst = newManyToManyJoins.ToList();

        //DataTypes by CrudRightTypes
        lst.AddRange(from dataType in DataSeederRepo.GetAll<DataType>()
            from crudRightType in
                DataSeederRepo.GetAll<CrudRightType>() //.Where(w=>w.CrtKey != nameof(ECrudOperationType.Confirm))
            select new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId,
                PKey = dataType.DtTable,
                CtId = crudRightTypeDataTypeId,
                CKey = crudRightType.CrtKey
            });

        //Menu by CrudRightTypes
        lst.AddRange(from mnu in DataSeederRepo.GetAll<MenuItm>().Where(w => w.MenLinkKey != "mdList")
            from crudRightType in
                DataSeederRepo.GetAll<CrudRightType>() //.Where(w=>w.CrtKey != nameof(ECrudOperationType.Confirm))
            select new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId,
                PKey = mnu.MenKey,
                CtId = crudRightTypeDataTypeId,
                CKey = crudRightType.CrtKey
            });

        var userByRoles = LoadFromJsonFile<UserByRole>(_secretDataFolder, "UsersByRoles.json");

        //users by roles
        lst.AddRange(userByRoles.Select(userByRoleModel => new ManyToManyJoin
        {
            PtId = userDataTypeId,
            PKey = userByRoleModel.UserName,
            CtId = roleDataTypeId,
            CKey = userByRoleModel.RoleName
        }));

        //admin meng
        string[] menGroupKeys = ["Main", "MasterData"]; //, "ProgramConstructor"
        lst.AddRange(menGroupKeys.Select(s => new ManyToManyJoin
        {
            PtId = roleDataTypeId, PKey = adminRoleKey, CtId = menuGroupDataTypeId, CKey = s
        }));

        //admin menu
        string[] menKeys =
        [
            //"DataInitEditor", 
            "DataTypes", "Roles", "Users", "MenuGroups", "MenuEditor", "Rights", "CrudRightTypes"
        ];
        lst.AddRange(menKeys.Select(s =>
            new ManyToManyJoin { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = menuDataTypeId, CKey = s }));

        return lst;
    }

    private List<ManyToManyJoin> GetThirdPartRights(string pairDKey, string firstDKey, string secondDKey)
    {
        var tempData = DataSeederTempData.Instance;

        var pairDataTypeId = tempData.GetIntIdByKey<DataType>(pairDKey);
        var roleDataTypeId = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<Role>());
        var firstDataTypeId = tempData.GetIntIdByKey<DataType>(firstDKey);
        var secondDataTypeId = tempData.GetIntIdByKey<DataType>(secondDKey);

        const string adminRoleKey = "Admin";

        var existingDataTypesToCrudTypes = _carcassRepo.GetManyToManyJoins(firstDataTypeId, secondDataTypeId);

        return existingDataTypesToCrudTypes.Select(s => new ManyToManyJoin
        {
            PtId = roleDataTypeId, PKey = adminRoleKey, CtId = pairDataTypeId, CKey = s.PKey + "." + s.CKey
        }).ToList();
    }

    private List<ManyToManyJoin> GetAdminRoleToDataTypes()
    {
        var tempData = DataSeederTempData.Instance;

        var dataTypeDt = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<DataType>());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<Role>());

        const string adminRoleKey = "Admin";

        var existingDataTypes = DataSeederRepo.GetAll<DataType>();

        return existingDataTypes.Select(s => new ManyToManyJoin
        {
            PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeDt, CKey = s.DtTable
        }).ToList();
    }

    private List<ManyToManyJoin> GetAdminRoleToMenuGroups()
    {
        var tempData = DataSeederTempData.Instance;

        var dataTypeMenuGroup = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<MenuGroup>());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<Role>());

        const string adminRoleKey = "Admin";

        var existingMenuGroups = DataSeederRepo.GetAll<MenuGroup>();

        return existingMenuGroups.Select(s => new ManyToManyJoin
        {
            PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeMenuGroup, CKey = s.MengKey
        }).ToList();
    }

    private List<ManyToManyJoin> GetAdminRoleToMenuItems()
    {
        var tempData = DataSeederTempData.Instance;

        var dataTypeMenu = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<MenuItm>());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<Role>());

        const string adminRoleKey = "Admin";

        var existingMenuItems = DataSeederRepo.GetAll<MenuItm>();

        return existingMenuItems.Select(s => new ManyToManyJoin
        {
            PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeMenu, CKey = s.MenKey
        }).ToList();
    }

    private List<ManyToManyJoin> GetMenuToDataTypes()
    {
        var tempData = DataSeederTempData.Instance;
        var dtMen = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<MenuItm>());
        var dtDt = tempData.GetIntIdByKey<DataType>(_carcassRepo.GetTableName<DataType>());

        var res = new List<ManyToManyJoin>();
        var existingMenu = DataSeederRepo.GetAll<MenuItm>();
        var existingDataTypes = DataSeederRepo.GetAll<DataType>();

        foreach (var miItm in existingMenu.Where(w =>
                     w.MenLinkKey == "mdList" && !string.IsNullOrWhiteSpace(w.MenValue)))
        {
            var dataType = existingDataTypes.SingleOrDefault(s => s.DtTable == miItm.MenValue);
            if (dataType == null)
                continue;
            res.Add(new ManyToManyJoin { PtId = dtMen, PKey = miItm.MenKey, CtId = dtDt, CKey = dataType.DtTable });
            var gm = dataType.DtGridRulesJson is null ? null : GridModel.DeserializeGridModel(dataType.DtGridRulesJson);
            if (gm == null)
                continue;
            foreach (var cell in gm.Cells)
            {
                var dtTable = cell switch
                {
                    LookupCell lookupCell => lookupCell.DataMember,
                    MdLookupCell mdLookupCell => mdLookupCell.DtTable,
                    _ => null
                };

                if (dtTable is null)
                    continue;

                dataType = existingDataTypes.SingleOrDefault(s => s.DtTable == dtTable);
                if (dataType == null)
                    continue;

                res.Add(new ManyToManyJoin { PtId = dtMen, PKey = miItm.MenKey, CtId = dtDt, CKey = dataType.DtTable });
            }
        }

        return res;
    }

    private List<ManyToManyJoin> GetMenuToDataTypesNeedLess()
    {
        var menuDKey = _carcassRepo.GetTableName<MenuItm>();
        var dataTypeDKey = _carcassRepo.GetTableName<DataType>();

        var tempData = DataSeederTempData.Instance;
        var dtMen = tempData.GetIntIdByKey<DataType>(menuDKey);
        var dtDt = tempData.GetIntIdByKey<DataType>(dataTypeDKey);

        var res = new List<ManyToManyJoin>();
        var existingManyToManyJoins = DataSeederRepo.GetAll<ManyToManyJoin>();

        res.AddRange(existingManyToManyJoins.Where(s => s.PtId == dtMen && s.CtId == dtDt));
        res.AddRange(existingManyToManyJoins.Where(s =>
            s.PtId == dtDt && s.PKey == menuDKey && s.CtId == dtDt && s.CKey == dataTypeDKey));

        return res;
    }
}