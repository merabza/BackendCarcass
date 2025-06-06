﻿using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom;
using CarcassMasterDataDom.CellModels;
using DatabaseToolsShared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

    private static JsonSerializerSettings SerializerSettings =>
        new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    //protected საჭიროა XxxNewDataTypesSeeder-ში
    // ReSharper disable once MemberCanBePrivate.Global
    protected static string SerializeGrid(GridModel gridModel)
    {
        return JsonConvert.SerializeObject(gridModel, SerializerSettings);
    }

    protected override bool AdditionalCheck(List<DataTypeSeederModel> jsonData, List<DataType> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<DataType>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return SetParents(jsonData, savedData) && SetParentDataTypes() && RemoveRedundantDataTypes();
    }

    protected override List<DataType> Adapt(List<DataTypeSeederModel> dataTypesSeedData)
    {
        return dataTypesSeedData.Select(s => new DataType
        {
            DtKey = s.DtKey,
            DtName = s.DtName,
            DtNameNominative = s.DtNameNominative,
            DtNameGenitive = s.DtNameGenitive,
            DtTable = s.DtTable,
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
            var oneRec = dataTypesList.SingleOrDefault(s => s.DtKey == dataTypeSeederModel.DtKey);
            if (oneRec == null) continue;

            oneRec.DtParentDataTypeId = tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtParentDataTypeIdDtKey!);
            forUpdate.Add(oneRec);
        }

        //DtManyToManyJoinParentDataTypeKey => DtManyToManyJoinParentDataTypeId
        foreach (var dataTypeSeederModel in dataTypesSeedData.Where(w => w.DtManyToManyJoinParentDataTypeKey != null))
        {
            var oneRec = dataTypesList.SingleOrDefault(s => s.DtKey == dataTypeSeederModel.DtKey);
            if (oneRec == null) continue;

            oneRec.DtManyToManyJoinParentDataTypeId =
                tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtManyToManyJoinParentDataTypeKey!);
            forUpdate.Add(oneRec);
        }

        //DtManyToManyJoinChildDataTypeKey => DtManyToManyJoinChildDataTypeId
        foreach (var dataTypeSeederModel in dataTypesSeedData.Where(w => w.DtManyToManyJoinChildDataTypeKey != null))
        {
            var oneRec = dataTypesList.SingleOrDefault(s => s.DtKey == dataTypeSeederModel.DtKey);
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

        var dataTypeId = tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.DataType.ToDtKey());

        var dtdt = new Tuple<int, int>[]
        {
            new(tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuItm.ToDtKey()),
                tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.MenuGroup.ToDtKey()))
        };

        var dtdtdt = new Tuple<int, int, int>[]
        {
            new(tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.DataTypeToDataType.ToDtKey()), dataTypeId,
                dataTypeId),
            new(tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.DataTypeToCrudType.ToDtKey()), dataTypeId,
                tempData.GetIntIdByKey<DataType>(ECarcassDataTypeKeys.CrudRightType.ToDtKey()))
        };

        return CarcassRepo.SetDtParentDataTypes(dtdt) && CarcassRepo.SetManyToManyJoinParentChildDataTypes(dtdtdt);
    }

    protected override List<DataType> CreateListByRules()
    {
        var appClaimDKey = ECarcassDataTypeKeys.AppClaim.ToDtKey();
        var crudRightTypeDKey = ECarcassDataTypeKeys.CrudRightType.ToDtKey();
        var newDataTypes = new DataType[]
        {
            //carcass used
            //AppClaim
            new()
            {
                DtKey = appClaimDKey,
                DtName = "სპეციალური უფლებები",
                DtNameNominative = "სპეციალური უფლება",
                DtNameGenitive = "სპეციალური უფლების",
                DtTable = DataSeederRepo.GetTableName<AppClaim>(),
                DtIdFieldName = nameof(AppClaim.AclId).UnCapitalize(),
                DtKeyFieldName = nameof(AppClaim.AclKey).UnCapitalize(),
                DtNameFieldName = nameof(AppClaim.AclName).UnCapitalize(),
                DtGridRulesJson = SerializeGrid(GetKeyNameGridModel(appClaimDKey))
            },
            //DataType
            new()
            {
                DtKey = ECarcassDataTypeKeys.DataType.ToDtKey(),
                DtName = "მონაცემთა ტიპები",
                DtNameNominative = "მონაცემთა ტიპი",
                DtNameGenitive = "მონაცემთა ტიპის",
                DtTable = DataSeederRepo.GetTableName<DataType>(),
                DtIdFieldName = nameof(DataType.DtId).UnCapitalize(),
                DtKeyFieldName = nameof(DataType.DtKey).UnCapitalize(),
                DtNameFieldName = nameof(DataType.DtName).UnCapitalize(),
                DtGridRulesJson = SerializeGrid(CreateDataTypesGridModel())
            },
            //dataTypeToCrudTypeModel
            new()
            {
                DtKey = ECarcassDataTypeKeys.DataTypeToCrudType.ToDtKey(),
                DtName = "მონაცემების ცვლილებაზე უფლებები",
                DtNameNominative = "მონაცემების ცვლილებაზე უფლება",
                DtNameGenitive = "მონაცემების ცვლილებაზე უფლების",
                DtTable = "dataTypesToCrudTypes"
            },
            //CrudRightType
            new()
            {
                DtKey = crudRightTypeDKey,
                DtName = "მონაცემების ცვლილებაზე უფლებების ტიპები",
                DtNameNominative = "მონაცემების ცვლილებაზე უფლების ტიპი",
                DtNameGenitive = "მონაცემების ცვლილებაზე უფლების ტიპის",
                DtTable = DataSeederRepo.GetTableName<CrudRightType>(),
                DtIdFieldName = nameof(CrudRightType.CrtId).UnCapitalize(),
                DtKeyFieldName = nameof(CrudRightType.CrtKey).UnCapitalize(),
                DtNameFieldName = nameof(CrudRightType.CrtName).UnCapitalize(),
                DtGridRulesJson = SerializeGrid(GetKeyNameGridModel(crudRightTypeDKey))
            },
            //DataTypeToDataTypeModel
            new()
            {
                DtKey = ECarcassDataTypeKeys.DataTypeToDataType.ToDtKey(),
                DtName = "უფლებები",
                DtNameNominative = "უფლება",
                DtNameGenitive = "უფლების",
                DtTable = "dataTypesToDataTypes"
            },
            //MenuGroup
            new()
            {
                DtKey = ECarcassDataTypeKeys.MenuGroup.ToDtKey(),
                DtName = "მენიუს ჯგუფები",
                DtNameNominative = "მენიუს ჯგუფი",
                DtNameGenitive = "მენიუს ჯგუფის",
                DtTable = DataSeederRepo.GetTableName<MenuGroup>(),
                DtIdFieldName = nameof(MenuGroup.MengId).UnCapitalize(),
                DtKeyFieldName = nameof(MenuGroup.MengKey).UnCapitalize(),
                DtNameFieldName = nameof(MenuGroup.MengName).UnCapitalize(),
                DtGridRulesJson = SerializeGrid(CreateMenuGroupsGridModel())
            },
            //MenuItm
            new()
            {
                DtKey = ECarcassDataTypeKeys.MenuItm.ToDtKey(),
                DtName = "მენიუ",
                DtNameNominative = "მენიუ",
                DtNameGenitive = "მენიუს",
                DtTable = DataSeederRepo.GetTableName<MenuItm>(),
                DtIdFieldName = nameof(MenuItm.MenId).UnCapitalize(),
                DtKeyFieldName = nameof(MenuItm.MenKey).UnCapitalize(),
                DtNameFieldName = nameof(MenuItm.MenName).UnCapitalize(),
                DtGridRulesJson = SerializeGrid(CreateMenuGridModel())
            },
            //Role
            new()
            {
                DtKey = ECarcassDataTypeKeys.Role.ToDtKey(),
                DtName = "როლები",
                DtNameNominative = "როლი",
                DtNameGenitive = "როლის",
                DtTable = DataSeederRepo.GetTableName<Role>(),
                DtIdFieldName = nameof(Role.RolId).UnCapitalize(),
                DtKeyFieldName = nameof(Role.RolKey).UnCapitalize(),
                DtNameFieldName = nameof(Role.RolName).UnCapitalize(),
                DtGridRulesJson = SerializeGrid(CreateRolesGridModel())
            },
            //User
            new()
            {
                DtKey = ECarcassDataTypeKeys.User.ToDtKey(),
                DtName = "მომხმარებლები",
                DtNameNominative = "მომხმარებელი",
                DtNameGenitive = "მომხმარებლის",
                DtTable = DataSeederRepo.GetTableName<User>(),
                DtIdFieldName = nameof(User.UsrId).UnCapitalize(),
                DtKeyFieldName = nameof(User.NormalizedUserName).UnCapitalize(),
                DtNameFieldName = nameof(User.FullName).UnCapitalize(),
                DtGridRulesJson = SerializeGrid(CreateUsersGridModel())
            }
        };

        return [.. newDataTypes];
    }

    private GridModel CreateDataTypesGridModel()
    {
        var gridModel = GetKeyNameGridModel(ECarcassDataTypeKeys.DataType.ToDtKey());
        var cells = new[]
        {
            GetTextBoxCell(nameof(DataType.DtNameNominative).UnCapitalize(), "სახელობითი"),
            GetTextBoxCell(nameof(DataType.DtNameGenitive).UnCapitalize(), "მიცემითი"),
            GetTextBoxCell(nameof(DataType.DtTable).UnCapitalize(), "ცხრილი"),
            GetTextBoxCell(nameof(DataType.DtIdFieldName).UnCapitalize(), "იდენტიფიკატორი ველის სახელი"),
            GetTextBoxCell(nameof(DataType.DtKeyFieldName).UnCapitalize(), "კოდი ველის სახელი"),
            GetTextBoxCell(nameof(DataType.DtNameFieldName).UnCapitalize(), "სახელი ველის სახელი"),
            GetMdComboCell(nameof(DataType.DtParentDataTypeId).UnCapitalize(), "უფლებების მშობელი",
                DataSeederRepo.GetTableName<DataType>())
        };
        gridModel.Cells.AddRange(cells);
        return gridModel;
    }

    private GridModel CreateMenuGridModel()
    {
        var gridModel = GetKeyNameSortIdGridModel(ECarcassDataTypeKeys.MenuItm.ToDtKey());
        var cells = new[]
        {
            GetTextBoxCell(nameof(MenuItm.MenValue).UnCapitalize(), "პარამეტრი"),
            GetMdComboCell(nameof(MenuItm.MenGroupId).UnCapitalize(), "ჯგუფი",
                DataSeederRepo.GetTableName<MenuGroup>()),
            GetTextBoxCell(nameof(MenuItm.MenLinkKey).UnCapitalize(), "ბმული"),
            GetTextBoxCell(nameof(MenuItm.MenIconName).UnCapitalize(), "ხატულა")
        };
        gridModel.Cells.AddRange(cells);
        return gridModel;
    }

    private static GridModel CreateMenuGroupsGridModel()
    {
        var gridModel = GetKeyNameSortIdGridModel(ECarcassDataTypeKeys.MenuGroup.ToDtKey());
        gridModel.Cells.Add(GetTextBoxCell(nameof(MenuGroup.MengIconName).UnCapitalize(), "ხატულა"));
        return gridModel;
    }

    private static GridModel CreateRolesGridModel()
    {
        var gridModel = GetKeyNameGridModel(ECarcassDataTypeKeys.Role.ToDtKey());
        gridModel.Cells.Add(GetIntegerCell(nameof(Role.RolLevel).UnCapitalize(), "დონე"));
        return gridModel;
    }

    private static GridModel CreateUsersGridModel()
    {
        var gridModel = new GridModel();
        var cells = new[]
        {
            GetAutoNumberColumn("usrId"),
            GetTextBoxCell(nameof(User.UserName).UnCapitalize(), "მომხმარებლის სახელი"),
            GetTextBoxCell(nameof(User.Email).UnCapitalize(), "ელექტრონული ფოსტის მისამართი"),
            GetTextBoxCell(nameof(User.FirstName).UnCapitalize(), "სახელი"),
            GetTextBoxCell(nameof(User.LastName).UnCapitalize(), "გვარი")
        };
        gridModel.Cells = [.. cells];
        return gridModel;
    }

    protected static GridModel GetKeyNameSortIdGridModel(string pref)
    {
        var gridModel = GetKeyNameGridModel(pref);
        gridModel.Cells.Add(GetSortIdCell());
        return gridModel;
    }

    protected static GridModel GetKeyNameGridModel(string pref)
    {
        var gridModel = new GridModel();
        var cells = new[]
        {
            GetAutoNumberColumn($"{pref}Id"), GetKeyColumn($"{pref}Key"), GetNameColumn($"{pref}Name")
        };
        gridModel.Cells = [.. cells];
        return gridModel;
    }

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