using CarcassDataSeeding.Models;
using CarcassDb.Domain;
using CarcassDb.Models;
using CarcassMasterDataDom;
using CarcassMasterDataDom.CellModels;
using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using SystemToolsShared.Errors;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class ManyToManyJoinsSeeder(string secretDataFolder, string dataSeedFolder, IDataSeederRepository repo)
    : DataSeeder<ManyToManyJoin>(dataSeedFolder, repo)
{
    protected override Option<Err[]> CreateByJsonFile()
    {
        //return Repo.CreateEntities(CreateManyToManyJoinsList(LoadFromJsonFile<ManyToManyJoinSeederModel>()));
        if (!Repo.CreateEntities(CreateManyToManyJoinsList(LoadFromJsonFile<ManyToManyJoinSeederModel>())))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "ManyToManyJoinEntitiesCannotBeCreated",
                    ErrorMessage = "ManyToManyJoin entities cannot be created"
                }
            };
        return null;
    }

    protected override Option<Err[]> AdditionalCheck()
    {
        var dataTypeDKey = ECarcassDataTypeKeys.DataType.ToDtKey();
        if (!Check(CreateMustList())
            && Check(GetThirdPartRights(ECarcassDataTypeKeys.DataTypeToDataType.ToDtKey(),
                dataTypeDKey,
                dataTypeDKey)) //rol admin mmj(dt,dt) all
            && Check(GetThirdPartRights(ECarcassDataTypeKeys.DataTypeToCrudType.ToDtKey(),
                dataTypeDKey,
                ECarcassDataTypeKeys.CrudRightType.ToDtKey())) //rol admin DataCrudRights all
            //&& Check(GetThirdPartRights(MenuToCrudTypeModel.DKey, MenuItm.DKey, CrudRightType.DKey)) //rol admin MenuCrudRights all
            && Check(GetAdminRoleToDataTypes()) //rol admin DataTypes all
            && Check(GetAdminRoleToMenuGroups()) //rol admin MenuGroups all
            && Check(GetAdminRoleToMenuItems()) //rol admin MenuItems all
            && Check(GetMenuToDataTypes()) //men -> DataTypes
            && Repo.RemoveNeedlessRecords(GetMenuToDataTypesNeedLess()))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "ManyToManyJoinEntitiesCannotBeChecked",
                    ErrorMessage = "ManyToManyJoin entities cannot be Checked"
                }
            };
        return null;
    }

    private bool Check(IReadOnlyCollection<ManyToManyJoin> mustBeDataTypes)
    {
        var existingManyToManyJoins = Repo.GetAll<ManyToManyJoin>()
            .Select(s => new ManyToManyJoinDomain(s.PtId, s.PKey, s.CtId, s.CKey)).ToList();
        if (mustBeDataTypes == null)
            return true;
        var forAdd = mustBeDataTypes.Select(s => new ManyToManyJoinDomain(s.PtId, s.PKey, s.CtId, s.CKey))
            .Except(existingManyToManyJoins).ToList();
        return Repo.CreateEntities(forAdd.Select(s => new ManyToManyJoin
            { PtId = s.PtId, PKey = s.PKey, CtId = s.CtId, CKey = s.CKey }).ToList());
    }

    private static List<ManyToManyJoin> CreateManyToManyJoinsList(
        IEnumerable<ManyToManyJoinSeederModel> manyToManyJoinsSeedData)
    {
        var tempData = DataSeederTempData.Instance;
        return manyToManyJoinsSeedData.Select(s => new ManyToManyJoin
        {
            PtId = tempData.GetIntIdByKey<DataType>(s.PtIdDtKey), PKey = s.PKey,
            CtId = tempData.GetIntIdByKey<DataType>(s.CtIdDtKey), CKey = s.CKey
        }).ToList();
    }

    protected override List<ManyToManyJoin> CreateMustList()
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
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = dataTypeDKey },
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = userDKey },
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = roleDKey },
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = menuDKey },
            new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId, PKey = dataTypeDKey, CtId = dataTypeDataTypeId, CKey = crudRightTypeDKey
            },
            //dt mnu dt [dt, crudRightType]
            new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId, PKey = menuDKey, CtId = dataTypeDataTypeId, CKey = crudRightTypeDKey
            },
            //dt usr dt rol
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = userDKey, CtId = dataTypeDataTypeId, CKey = roleDKey },
            //dt rol dt [meng, men, dt, DataTypeToDataTypeModel, DataTypeToCrudTypeModel, rol]
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = menuGroupDKey },
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = menuDKey },
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = dataTypeDKey },
            new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId,
                CKey = ECarcassDataTypeKeys.DataTypeToDataType.ToDtKey()
            },
            new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId,
                CKey = ECarcassDataTypeKeys.DataTypeToCrudType.ToDtKey()
            },
            new ManyToManyJoin
                { PtId = dataTypeDataTypeId, PKey = roleDKey, CtId = dataTypeDataTypeId, CKey = roleDKey },
            //admin dt dt [dt, usr, rol]
            new ManyToManyJoin
                { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = dataTypeDKey },
            new ManyToManyJoin
                { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = userDKey },
            new ManyToManyJoin
                { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = dataTypeDataTypeId, CKey = roleDKey }
        ];
        var lst = newManyToManyJoins.ToList();

        //DataTypes by CrudRightTypes
        lst.AddRange(
            from dataType in Repo.GetAll<DataType>()
            from crudRightType in
                Repo.GetAll<CrudRightType>() //.Where(w=>w.CrtKey != nameof(ECrudOperationType.Confirm))
            select new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId, PKey = dataType.DtKey, CtId = crudRightTypeDataTypeId,
                CKey = crudRightType.CrtKey
            });

        //Menu by CrudRightTypes
        lst.AddRange(
            from mnu in Repo.GetAll<MenuItm>().Where(w => w.MenLinkKey != "mdList")
            from crudRightType in
                Repo.GetAll<CrudRightType>() //.Where(w=>w.CrtKey != nameof(ECrudOperationType.Confirm))
            select new ManyToManyJoin
            {
                PtId = dataTypeDataTypeId, PKey = mnu.MenKey, CtId = crudRightTypeDataTypeId,
                CKey = crudRightType.CrtKey
            });

        var userByRoles = LoadFromJsonFile<UserByRole>(secretDataFolder, "UsersByRoles.json");

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
            { PtId = roleDataTypeId, PKey = adminRoleKey, CtId = menuGroupDataTypeId, CKey = s }));

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

        var existingDataTypesToCrudTypes = Repo.GetManyToManyJoins(firstDataTypeId, secondDataTypeId);

        return existingDataTypesToCrudTypes.Select(s => new ManyToManyJoin
        {
            PtId = roleDataTypeId,
            PKey = adminRoleKey,
            CtId = pairDataTypeId,
            CKey = s.PKey + "." + s.CKey
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
            { PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeDt, CKey = s.DtKey }).ToList();
    }

    private List<ManyToManyJoin> GetAdminRoleToMenuGroups()
    {
        var tempData = DataSeederTempData.Instance;

        var dataTypeMenuGroup =
            tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuGroup.ToDtKey());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.Role.ToDtKey());

        const string adminRoleKey = "Admin";

        var existingMenuGroups = Repo.GetAll<MenuGroup>();


        return existingMenuGroups.Select(s => new ManyToManyJoin
            { PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeMenuGroup, CKey = s.MengKey }).ToList();
    }

    private List<ManyToManyJoin> GetAdminRoleToMenuItems()
    {
        var tempData = DataSeederTempData.Instance;

        var dataTypeMenu = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuItm.ToDtKey());
        var dataTypeRol = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.Role.ToDtKey());

        const string adminRoleKey = "Admin";

        var existingMenuItems = Repo.GetAll<MenuItm>();

        return existingMenuItems.Select(s => new ManyToManyJoin
            { PtId = dataTypeRol, PKey = adminRoleKey, CtId = dataTypeMenu, CKey = s.MenKey }).ToList();
    }

    private List<ManyToManyJoin> GetMenuToDataTypes()
    {
        var tempData = DataSeederTempData.Instance;
        var dtmen = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuItm.ToDtKey());
        var dtdt = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.DataType.ToDtKey());

        var res = new List<ManyToManyJoin>();
        var existingMenu = Repo.GetAll<MenuItm>();
        var existingDataTypes = Repo.GetAll<DataType>();

        foreach (var miItm in existingMenu.Where(
                     w => w.MenLinkKey == "mdList" && !string.IsNullOrWhiteSpace(w.MenValue)))
        {
            var dataType = existingDataTypes.SingleOrDefault(s => s.DtTable == miItm.MenValue);
            if (dataType == null)
                continue;
            res.Add(new ManyToManyJoin { PtId = dtmen, PKey = miItm.MenKey, CtId = dtdt, CKey = dataType.DtKey });
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

                res.Add(new ManyToManyJoin { PtId = dtmen, PKey = miItm.MenKey, CtId = dtdt, CKey = dataType.DtKey });
            }
        }

        return res;
    }


    private List<ManyToManyJoin> GetMenuToDataTypesNeedLess()
    {
        var menuDKey = ECarcassDataTypeKeys.MenuItm.ToDtKey();
        var dataTypeDKey = ECarcassDataTypeKeys.DataType.ToDtKey();

        var tempData = DataSeederTempData.Instance;
        var dtmen = tempData.GetIntIdByKey<DataType>(menuDKey);
        var dtdt = tempData.GetIntIdByKey<DataType>(dataTypeDKey);

        var res = new List<ManyToManyJoin>();
        var existingManyToManyJoins = Repo.GetAll<ManyToManyJoin>();

        res.AddRange(existingManyToManyJoins.Where(s => s.PtId == dtmen && s.CtId == dtdt));
        res.AddRange(existingManyToManyJoins.Where(s =>
            s.PtId == dtdt && s.PKey == menuDKey && s.CtId == dtdt && s.CKey == dataTypeDKey));

        return res;
    }
}