using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Fabrics;
using CarcassDb.Models;
using CarcassMasterDataDom.CellModels;
using DatabaseToolsShared;
using SystemToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class DataTypesSeeder : DataSeeder<DataType, DataTypeSeederModel>
{
    protected readonly ICarcassDataSeederRepository CarcassRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypesSeeder(ICarcassDataSeederRepository carcassRepo, string dataSeedFolder, IDataSeederRepository repo,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, seedDataType, keyFieldNamesList)
    {
        CarcassRepo = carcassRepo;
    }

    //private static JsonSerializerSettings SerializerSettings =>
    //    new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    //protected საჭიროა XxxNewDataTypesSeeder-ში
    // ReSharper disable once MemberCanBePrivate.Global
    //protected static string SerializeGrid(GridModel gridModel)
    //{
    //    return JsonConvert.SerializeObject(gridModel, SerializerSettings);
    //}

    protected override bool AdditionalCheck(List<DataTypeSeederModel> jsonData, List<DataType> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<DataType>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return SetParents(jsonData, savedData) && SetParentDataTypes() && RemoveRedundantDataTypes();
    }

    protected override List<DataType> Adapt(List<DataTypeSeederModel> dataTypesSeedData)
    {
        return dataTypesSeedData.Select(s => new DataType
        {
            DtTable = s.DtTable,
            DtName = s.DtName,
            DtNameNominative = s.DtNameNominative,
            DtNameGenitive = s.DtNameGenitive,
            DtIdFieldName = s.DtIdFieldName,
            DtKeyFieldName = s.DtKeyFieldName,
            DtNameFieldName = s.DtNameFieldName,
            DtGridRulesJson = s.DtGridRulesJson
        }).ToList();
    }

    private bool SetParents(IReadOnlyCollection<DataTypeSeederModel> dataTypesSeedData,
        IReadOnlyCollection<DataType> dataTypesList)
    {
        var tempData = DataSeederTempData.Instance;
        var forUpdate = new List<DataType>();

        //DtParentDataTypeIdDtKey => DtParentDataTypeId
        foreach (var dataTypeSeederModel in dataTypesSeedData.Where(w => w.DtParentDataTypeIdDtKey != null))
        {
            var oneRec = dataTypesList.SingleOrDefault(s => s.DtTable == dataTypeSeederModel.DtTable);
            if (oneRec == null) continue;

            oneRec.DtParentDataTypeId = tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtParentDataTypeIdDtKey!);
            forUpdate.Add(oneRec);
        }

        //DtManyToManyJoinParentDataTypeKey => DtManyToManyJoinParentDataTypeId
        foreach (var dataTypeSeederModel in dataTypesSeedData.Where(w => w.DtManyToManyJoinParentDataTypeKey != null))
        {
            var oneRec = dataTypesList.SingleOrDefault(s => s.DtTable == dataTypeSeederModel.DtTable);
            if (oneRec == null) continue;

            oneRec.DtManyToManyJoinParentDataTypeId =
                tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtManyToManyJoinParentDataTypeKey!);
            forUpdate.Add(oneRec);
        }

        //DtManyToManyJoinChildDataTypeKey => DtManyToManyJoinChildDataTypeId
        foreach (var dataTypeSeederModel in dataTypesSeedData.Where(w => w.DtManyToManyJoinChildDataTypeKey != null))
        {
            var oneRec = dataTypesList.SingleOrDefault(s => s.DtTable == dataTypeSeederModel.DtTable);
            if (oneRec == null) continue;

            oneRec.DtManyToManyJoinChildDataTypeId =
                tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtManyToManyJoinChildDataTypeKey!);
            forUpdate.Add(oneRec);
        }

        return DataSeederRepo.SetUpdates(forUpdate);
    }

    protected virtual bool RemoveRedundantDataTypes()
    {
        var toRemoveTableNames = new[] { "dataRights", "dataRightTypes", "forms" };
        return CarcassRepo.RemoveRedundantDataTypesByTableNames(toRemoveTableNames);
    }

    protected virtual bool SetParentDataTypes()
    {
        var tempData = DataSeederTempData.Instance;
        var dataTypeTableName = CarcassRepo.GetTableName<DataType>();
        var crudRightTypeTableName = CarcassRepo.GetTableName<CrudRightType>();
        var dataTypeId = tempData.GetIntIdByKey<DataType>(dataTypeTableName);

        var dtdt = new Tuple<int, int>[]
        {
            new(tempData.GetIntIdByKey<DataType>(CarcassRepo.GetTableName<MenuItm>()),
                tempData.GetIntIdByKey<DataType>(CarcassRepo.GetTableName<MenuGroup>()))
        };

        var dtdtdt = new Tuple<int, int, int>[]
        {
            new(tempData.GetIntIdByKey<DataType>($"{dataTypeTableName}{dataTypeTableName}"), dataTypeId,
                dataTypeId),
            new(tempData.GetIntIdByKey<DataType>($"{dataTypeTableName}{crudRightTypeTableName}"), dataTypeId,
                tempData.GetIntIdByKey<DataType>(crudRightTypeTableName))
        };

        return CarcassRepo.SetDtParentDataTypes(dtdt) && CarcassRepo.SetManyToManyJoinParentChildDataTypes(dtdtdt);
    }

    protected override List<DataType> CreateListByRules()
    {
        //var appClaimDKey = ECarcassDataTypeKeys.AppClaim.ToDtKey();
        //var crudRightTypeDKey = ECarcassDataTypeKeys.CrudRightType.ToDtKey();
        var newDataTypes = new[]
        {
            //carcass used

            //AppClaim
            DataTypeFabric.Create("სპეციალური უფლებები", "სპეციალური უფლება", "სპეციალური უფლების",
                CarcassRepo.GetTableName<AppClaim>(), nameof(AppClaim.AclId).UnCapitalize(),
                nameof(AppClaim.AclKey).UnCapitalize(), nameof(AppClaim.AclName).UnCapitalize()),

            //DataType
            DataTypeFabric.Create("მონაცემთა ტიპები", "მონაცემთა ტიპი", "მონაცემთა ტიპის",
                CarcassRepo.GetTableName<DataType>(), nameof(DataType.DtId).UnCapitalize(),
                nameof(DataType.DtTable).UnCapitalize(), nameof(DataType.DtName).UnCapitalize(),
                GetTextBoxCell(nameof(DataType.DtNameNominative).UnCapitalize(), "სახელობითი"),
                GetTextBoxCell(nameof(DataType.DtNameGenitive).UnCapitalize(), "მიცემითი"),
                GetTextBoxCell(nameof(DataType.DtTable).UnCapitalize(), "ცხრილი"),
                GetTextBoxCell(nameof(DataType.DtIdFieldName).UnCapitalize(), "იდენტიფიკატორი ველის სახელი"),
                GetTextBoxCell(nameof(DataType.DtKeyFieldName).UnCapitalize(), "კოდი ველის სახელი"),
                GetTextBoxCell(nameof(DataType.DtNameFieldName).UnCapitalize(), "სახელი ველის სახელი"),
                GetMdComboCell(nameof(DataType.DtParentDataTypeId).UnCapitalize(), "უფლებების მშობელი",
                    CarcassRepo.GetTableName<DataType>())),

            //dataTypeToCrudTypeModel
            DataTypeFabric.CreatePseudo("მონაცემების ცვლილებაზე უფლებები", "მონაცემების ცვლილებაზე უფლება",
                "მონაცემების ცვლილებაზე უფლების", CarcassRepo.GetTableName<DataType>(),
                CarcassRepo.GetTableName<CrudRightType>()),

            //CrudRightType
            DataTypeFabric.Create("მონაცემების ცვლილებაზე უფლებების ტიპები", "მონაცემების ცვლილებაზე უფლების ტიპი",
                "მონაცემების ცვლილებაზე უფლების ტიპის", CarcassRepo.GetTableName<CrudRightType>(),
                nameof(CrudRightType.CrtId).UnCapitalize(), nameof(CrudRightType.CrtKey).UnCapitalize(),
                nameof(CrudRightType.CrtName).UnCapitalize()),

            //DataTypeToDataTypeModel
            DataTypeFabric.CreatePseudo("უფლებები", "უფლება", "უფლების", CarcassRepo.GetTableName<DataType>(),
                CarcassRepo.GetTableName<DataType>()),

            //MenuGroup
            DataTypeFabric.Create("მენიუს ჯგუფები", "მენიუს ჯგუფი", "მენიუს ჯგუფის",
                CarcassRepo.GetTableName<MenuGroup>(), nameof(MenuGroup.MengId).UnCapitalize(),
                nameof(MenuGroup.MengKey).UnCapitalize(), nameof(MenuGroup.MengName).UnCapitalize(), GetSortIdCell(),
                GetTextBoxCell(nameof(MenuGroup.MengIconName).UnCapitalize(), "ხატულა")),

            //MenuItm
            DataTypeFabric.Create("მენიუ", "მენიუ", "მენიუს", CarcassRepo.GetTableName<MenuItm>(),
                nameof(MenuItm.MenId).UnCapitalize(), nameof(MenuItm.MenKey).UnCapitalize(),
                nameof(MenuItm.MenName).UnCapitalize(), GetSortIdCell(),
                GetTextBoxCell(nameof(MenuItm.MenValue).UnCapitalize(), "პარამეტრი"),
                GetMdComboCell(nameof(MenuItm.MenGroupId).UnCapitalize(), "ჯგუფი",
                    CarcassRepo.GetTableName<MenuGroup>()),
                GetTextBoxCell(nameof(MenuItm.MenLinkKey).UnCapitalize(), "ბმული"),
                GetTextBoxCell(nameof(MenuItm.MenIconName).UnCapitalize(), "ხატულა")),

            //Role
            DataTypeFabric.Create("როლები", "როლი", "როლის", CarcassRepo.GetTableName<Role>(),
                nameof(Role.RolId).UnCapitalize(), nameof(Role.RolKey).UnCapitalize(),
                nameof(Role.RolName).UnCapitalize(), GetIntegerCell(nameof(Role.RolLevel).UnCapitalize(), "დონე")),

            //User
            DataTypeFabric.Create("მომხმარებლები", "მომხმარებელი", "მომხმარებლის", CarcassRepo.GetTableName<User>(),
                nameof(User.UsrId).UnCapitalize(), nameof(User.NormalizedUserName).UnCapitalize(),
                nameof(User.FullName).UnCapitalize(),
                GetTextBoxCell(nameof(User.UserName).UnCapitalize(), "მომხმარებლის სახელი"),
                GetTextBoxCell(nameof(User.Email).UnCapitalize(), "ელექტრონული ფოსტის მისამართი"),
                GetTextBoxCell(nameof(User.FirstName).UnCapitalize(), "სახელი"),
                GetTextBoxCell(nameof(User.LastName).UnCapitalize(), "გვარი"))
        };

        return [.. newDataTypes];
    }

    //private GridModel CreateDataTypesGridModel()
    //{
    //    var gridModel = GetKeyNameGridModel(nameof(DataType.DtId), nameof(DataType.DtTable), nameof(DataType.DtName));
    //    var cells = new[]
    //    {
    //        GetTextBoxCell(nameof(DataType.DtNameNominative).UnCapitalize(), "სახელობითი"),
    //        GetTextBoxCell(nameof(DataType.DtNameGenitive).UnCapitalize(), "მიცემითი"),
    //        GetTextBoxCell(nameof(DataType.DtTable).UnCapitalize(), "ცხრილი"),
    //        GetTextBoxCell(nameof(DataType.DtIdFieldName).UnCapitalize(), "იდენტიფიკატორი ველის სახელი"),
    //        GetTextBoxCell(nameof(DataType.DtKeyFieldName).UnCapitalize(), "კოდი ველის სახელი"),
    //        GetTextBoxCell(nameof(DataType.DtNameFieldName).UnCapitalize(), "სახელი ველის სახელი"),
    //        GetMdComboCell(nameof(DataType.DtParentDataTypeId).UnCapitalize(), "უფლებების მშობელი",
    //            CarcassRepo.GetTableName<DataType>())
    //    };
    //    gridModel.Cells.AddRange(cells);
    //    return gridModel;
    //}

    //private GridModel CreateMenuGridModel()
    //{
    //    var gridModel =
    //        GetKeyNameSortIdGridModel(nameof(MenuItm.MenId), nameof(MenuItm.MenKey), nameof(MenuItm.MenName));
    //    var cells = new[]
    //    {
    //        GetTextBoxCell(nameof(MenuItm.MenValue).UnCapitalize(), "პარამეტრი"),
    //        GetMdComboCell(nameof(MenuItm.MenGroupId).UnCapitalize(), "ჯგუფი",
    //            CarcassRepo.GetTableName<MenuGroup>()),
    //        GetTextBoxCell(nameof(MenuItm.MenLinkKey).UnCapitalize(), "ბმული"),
    //        GetTextBoxCell(nameof(MenuItm.MenIconName).UnCapitalize(), "ხატულა")
    //    };
    //    gridModel.Cells.AddRange(cells);
    //    return gridModel;
    //}

    //private static GridModel CreateMenuGroupsGridModel()
    //{
    //    var gridModel = GetKeyNameSortIdGridModel(nameof(MenuGroup.MengId), nameof(MenuGroup.MengKey),
    //        nameof(MenuGroup.MengName));
    //    gridModel.Cells.Add(GetTextBoxCell(nameof(MenuGroup.MengIconName).UnCapitalize(), "ხატულა"));
    //    return gridModel;
    //}

    //private static GridModel CreateRolesGridModel()
    //{
    //    var gridModel = GetKeyNameGridModel(nameof(Role.RolId), nameof(Role.RolKey), nameof(Role.RolName));
    //    gridModel.Cells.Add(GetIntegerCell(nameof(Role.RolLevel).UnCapitalize(), "დონე"));
    //    return gridModel;
    //}

    //private static GridModel CreateUsersGridModel()
    //{
    //    var gridModel = new GridModel();
    //    var cells = new[]
    //    {
    //        GetAutoNumberColumn("usrId"),
    //        GetTextBoxCell(nameof(User.UserName).UnCapitalize(), "მომხმარებლის სახელი"),
    //        GetTextBoxCell(nameof(User.Email).UnCapitalize(), "ელექტრონული ფოსტის მისამართი"),
    //        GetTextBoxCell(nameof(User.FirstName).UnCapitalize(), "სახელი"),
    //        GetTextBoxCell(nameof(User.LastName).UnCapitalize(), "გვარი")
    //    };
    //    gridModel.Cells = [.. cells];
    //    return gridModel;
    //}

    //protected static GridModel GetKeyNameSortIdGridModel(string idFieldName, string keyFieldName, string nameFieldName)
    //{
    //    var gridModel = GetKeyNameGridModel(idFieldName, keyFieldName, nameFieldName);
    //    gridModel.Cells.Add(GetSortIdCell());
    //    return gridModel;
    //}

    //protected static GridModel GetKeyNameGridModel(string idFieldName, string keyFieldName, string nameFieldName)
    //{
    //    var gridModel = new GridModel();
    //    var cells = new[]
    //    {
    //        GetAutoNumberColumn(idFieldName), GetKeyColumn(keyFieldName), GetNameColumn(nameFieldName)
    //    };
    //    gridModel.Cells = [.. cells];
    //    return gridModel;
    //}

    protected static Cell GetAutoNumberColumn(string fieldName)
    {
        return Cell.Integer(fieldName, null, "", "", false).Default();
    }

    protected static Cell GetKeyColumn(string fieldName)
    {
        return GetTextBoxCell(fieldName, "კოდი");
    }

    protected static Cell GetNameColumn(string fieldName)
    {
        return GetTextBoxCell(fieldName, "სახელი");
    }

    protected static Cell GetTextBoxCell(string fieldName, string caption, bool allowNull = false)
    {
        return allowNull
            ? Cell.String(fieldName, caption).Nullable().Default()
            : Cell.String(fieldName, caption).Required($"{caption} შევსებული უნდა იყოს").Default();
    }

    //GetNumberColumn(3,"mrPosition","პოზიცია")
    protected static Cell GetIntegerCell(string fieldName, string caption, bool allowNull = false, bool isShort = false)
    {
        var res = allowNull
            ? Cell.Integer(fieldName, caption).Nullable().Min(0)
            : Cell.Integer(fieldName, caption).Required($"{caption} შევსებული უნდა იყოს").Default();
        return isShort ? res.Short() : res;
    }

    protected static Cell GetSortIdCell()
    {
        return Cell.Integer("sortId", "რიგითი ნომერი").Required("რიგითი ნომერი შევსებული უნდა იყოს").Default(-1).Min(-1)
            .SortId();
    }

    protected static Cell GetMdComboCell(string fieldName, string caption, string dtTable, bool allowNull = false)
    {
        var cell = Cell.MdLookup(fieldName, caption, dtTable).Default();
        return allowNull
            ? cell.Nullable()
            : cell.Positive($"{caption} არჩეული უნდა იყოს").Required($"{caption} არჩეული უნდა იყოს");
    }

    protected static Cell GetComboCellWithRowSource(string fieldName, string caption, string rowSource,
        bool isShort = false)
    {
        var res = Cell.RsLookup(fieldName, caption, rowSource).Default();
        return isShort ? res.Short() : res;
    }

    protected static Cell GetCheckBoxCell(string fieldName, string caption)
    {
        return Cell.Boolean(fieldName, caption);
    }
}