using System.Collections.Generic;
using System.Linq;
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

    protected override bool AdditionalCheck(List<ManyToManyJoinSeederModel> jsonData, List<ManyToManyJoin> savedData)
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

        return Repo.RemoveNeedlessRecords(GetMenuToDataTypesNeedLess());
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

    protected override List<ManyToManyJoin> Adapt(List<ManyToManyJoinSeederModel> manyToManyJoinsSeedData)
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

    protected override List<ManyToManyJoin> CreateListByRules()
    {
        var manyToManyJoinsList = CreateMustListByRules();

        var dataTypeDKey = ECarcassDataTypeKeys.DataType.ToDtKey();
        manyToManyJoinsList.AddRange(GetThirdPartRights(ECarcassDataTypeKeys.DataTypeToDataType.ToDtKey(), dataTypeDKey,
            dataTypeDKey));

        manyToManyJoinsList.AddRange(GetThirdPartRights(ECarcassDataTypeKeys.DataTypeToCrudType.ToDtKey(), dataTypeDKey,
            ECarcassDataTypeKeys.CrudRightType.ToDtKey()));

        manyToManyJoinsList.AddRange(GetAdminRoleToDataTypes());
        manyToManyJoinsList.AddRange(GetAdminRoleToMenuGroups());
        manyToManyJoinsList.AddRange(GetAdminRoleToMenuItems());
        manyToManyJoinsList.AddRange(GetMenuToDataTypes());

        return manyToManyJoinsList.Distinct().ToList();
    }

    protected virtual List<ManyToManyJoin> CreateMustListByRules()
    {
        var dataTypeDKey = ECarcassDataTypeKeys.DataType.ToDtKey();
        var userDKey = ECarcassDataTypeKeys.User.ToDtKey();
        var roleDKey = ECarcassDataTypeKeys.Role.ToDtKey();
        var menuGroupDKey = ECarcassDataTypeKeys.MenuGroup.ToDtKey();
        var menuDKey = ECarcassDataTypeKeys.MenuItm.ToDtKey();
        var crudRightTypeDKey = ECarcassDataTypeKeys.CrudRightType.ToDtKey();

        var tempData = DataSeederTempData.Instance;
        var dataTypeDataTypeId = tempData.GetIntIdByKey<DataType>(dataTypeDKey);
        var userDataTypeId = tempData.GetIntIdByKey<DataType>(userDKey);
        var roleDataTypeId = tempData.GetIntIdByKey<DataType>(roleDKey);
        var menuGroupDataTypeId = tempData.GetIntIdByKey<DataType>(menuGroupDKey);
        var menuDataTypeId = tempData.GetIntIdByKey<DataType>(menuDKey);
        var crudRightTypeDataTypeId = tempData.GetIntIdByKey<DataType>(crudRightTypeDKey);

        const string adminRoleKey = "Admin";

        ManyToManyJoin[] newManyToManyJoins =
        [
            //dt dt dt [dt, usr, role, menu, crudRightType]
            new() { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = dataTypeDKey },
            new() { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = userDKey },
            new() { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = roleDKey },
            new() { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = menuDKey },
            new()
            {
                PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = crudRightTypeDKey
            },
            //dt mnu dt [dt, crudRightType]
            new() { PtId = dataTypeDataTypeId, PKey = menuDKey, CtId = dataTypeDataTypeId, CKey = crudRightTypeDKey },
            //dt usr dt rol
            new() { PtId = dataTypeDataTypeId, PKey = userDKey, CtId = dataTypeDataTypeId, CKey = roleDKey },
            //dt rol dt [meng, men, dt, DataTypeToDataTypeModel, DataTypeToCrudTypeModel, rol]
            new() { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = menuGroupDKey },
            new() { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = menuDKey },
            new() { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = dataTypeDKey },
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = roleDKey,
                CtId = dataTypeDataTypeId,
                CKey = ECarcassDataTypeKeys.DataTypeToDataType.ToDtKey()
            },
            new()
            {
                PtId = dataTypeDataTypeId,
                PKey = roleDKey,
                CtId = dataTypeDataTypeId,
                CKey = ECarcassDataTypeKeys.DataTypeToCrudType.ToDtKey()
            },
            new() { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = roleDKey },
            //admin dt dt [dt, usr, rol]
            new() { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = dataTypeDKey },
            new() { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = userDKey },
            new() { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = roleDKey }
        ];
        var lst = newManyToManyJoins.ToList();

        //DataTypes by CrudRightTypes
        lst.AddRange(from dataType in Repo.GetAll<DataType>()
            from crudRightType in
                Repo.GetAll<CrudRightType>() //.Where(w=>w.CrtKey != nameof(ECrudOperationType.Confirm))
            select new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId,
                PKey = dataType.DtKey,
                CtId = crudRightTypeDataTypeId,
                CKey = crudRightType.CrtKey
            });

        //Menu by CrudRightTypes
        lst.AddRange(from mnu in Repo.GetAll<MenuItm>().Where(w => w.MenLinkKey != "mdList")
            from crudRightType in
                Repo.GetAll<CrudRightType>() //.Where(w=>w.CrtKey != nameof(ECrudOperationType.Confirm))
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
        var roleDataTypeId = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.Role.ToDtKey());
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

        var dataTypeDt = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.DataType.ToDtKey());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.Role.ToDtKey());

        const string adminRoleKey = "Admin";

        var existingDataTypes = Repo.GetAll<DataType>();

        return existingDataTypes.Select(s => new ManyToManyJoin
        {
            PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeDt, CKey = s.DtKey
        }).ToList();
    }

    private List<ManyToManyJoin> GetAdminRoleToMenuGroups()
    {
        var tempData = DataSeederTempData.Instance;

        var dataTypeMenuGroup = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuGroup.ToDtKey());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.Role.ToDtKey());

        const string adminRoleKey = "Admin";

        var existingMenuGroups = Repo.GetAll<MenuGroup>();

        return existingMenuGroups.Select(s => new ManyToManyJoin
        {
            PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeMenuGroup, CKey = s.MengKey
        }).ToList();
    }

    private List<ManyToManyJoin> GetAdminRoleToMenuItems()
    {
        var tempData = DataSeederTempData.Instance;

        var dataTypeMenu = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuItm.ToDtKey());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.Role.ToDtKey());

        const string adminRoleKey = "Admin";

        var existingMenuItems = Repo.GetAll<MenuItm>();

        return existingMenuItems.Select(s => new ManyToManyJoin
        {
            PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeMenu, CKey = s.MenKey
        }).ToList();
    }

    private List<ManyToManyJoin> GetMenuToDataTypes()
    {
        var tempData = DataSeederTempData.Instance;
        var dtMen = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuItm.ToDtKey());
        var dtDt = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.DataType.ToDtKey());

        var res = new List<ManyToManyJoin>();
        var existingMenu = Repo.GetAll<MenuItm>();
        var existingDataTypes = Repo.GetAll<DataType>();

        foreach (var miItm in existingMenu.Where(w =>
                     w.MenLinkKey == "mdList" && !string.IsNullOrWhiteSpace(w.MenValue)))
        {
            var dataType = existingDataTypes.SingleOrDefault(s => s.DtTable == miItm.MenValue);
            if (dataType == null)
                continue;
            res.Add(new ManyToManyJoin { PtId = dtMen, PKey = miItm.MenKey, CtId = dtDt, CKey = dataType.DtKey });
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

                res.Add(new ManyToManyJoin { PtId = dtMen, PKey = miItm.MenKey, CtId = dtDt, CKey = dataType.DtKey });
            }
        }

        return res;
    }

    private List<ManyToManyJoin> GetMenuToDataTypesNeedLess()
    {
        var menuDKey = ECarcassDataTypeKeys.MenuItm.ToDtKey();
        var dataTypeDKey = ECarcassDataTypeKeys.DataType.ToDtKey();

        var tempData = DataSeederTempData.Instance;
        var dtMen = tempData.GetIntIdByKey<DataType>(menuDKey);
        var dtDt = tempData.GetIntIdByKey<DataType>(dataTypeDKey);

        var res = new List<ManyToManyJoin>();
        var existingManyToManyJoins = Repo.GetAll<ManyToManyJoin>();

        res.AddRange(existingManyToManyJoins.Where(s => s.PtId == dtMen && s.CtId == dtDt));
        res.AddRange(existingManyToManyJoins.Where(s =>
            s.PtId == dtDt && s.PKey == menuDKey && s.CtId == dtDt && s.CKey == dataTypeDKey));

        return res;
    }
}