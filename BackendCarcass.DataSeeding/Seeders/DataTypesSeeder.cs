using System;
using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Database.Factories;
using BackendCarcass.Database.Models;
using BackendCarcass.DataSeeding.Models;
using BackendCarcass.MasterData.CellModels;
using SystemTools.DatabaseToolsShared;
using SystemTools.DomainShared.Repositories;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.DataSeeding.Seeders;

public /*open*/
    class DataTypesSeeder : DataSeeder<DataType, DataTypeSeederModel>
{
    protected readonly ICarcassDataSeederRepository CarcassRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypesSeeder(ICarcassDataSeederRepository carcassRepo, string dataSeedFolder, IDataSeederRepository repo,
        IUnitOfWork unitOfWork, ESeedDataType seedDataType = ESeedDataType.OnlyRules,
        List<string>? keyFieldNamesList = null) : base(dataSeedFolder, repo, unitOfWork, seedDataType,
        keyFieldNamesList)
    {
        CarcassRepo = carcassRepo;
    }

    public override bool AdditionalCheck(List<DataTypeSeederModel> jsonData, List<DataType> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<DataType>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return SetParents(jsonData, savedData) && SetParentDataTypes() && RemoveRedundantDataTypes();
    }

    public override List<DataType> Adapt(List<DataTypeSeederModel> dataTypesSeedData)
    {
        return dataTypesSeedData.Select(s => new DataType
        {
            //DtKey = s.DtKey,
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
        foreach (DataTypeSeederModel dataTypeSeederModel in dataTypesSeedData.Where(w =>
                     w.DtParentDataTypeIdDtKey != null))
        {
            DataType? oneRec = dataTypesList.SingleOrDefault(s => s.DtTable == dataTypeSeederModel.DtTable);
            if (oneRec == null)
            {
                continue;
            }

            oneRec.DtParentDataTypeId = tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtParentDataTypeIdDtKey!);
            forUpdate.Add(oneRec);
        }

        //DtManyToManyJoinParentDataTypeKey => DtManyToManyJoinParentDataTypeId
        foreach (DataTypeSeederModel dataTypeSeederModel in dataTypesSeedData.Where(w =>
                     w.DtManyToManyJoinParentDataTypeKey != null))
        {
            DataType? oneRec = dataTypesList.SingleOrDefault(s => s.DtTable == dataTypeSeederModel.DtTable);
            if (oneRec == null)
            {
                continue;
            }

            oneRec.DtManyToManyJoinParentDataTypeId =
                tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtManyToManyJoinParentDataTypeKey!);
            forUpdate.Add(oneRec);
        }

        //DtManyToManyJoinChildDataTypeKey => DtManyToManyJoinChildDataTypeId
        foreach (DataTypeSeederModel dataTypeSeederModel in dataTypesSeedData.Where(w =>
                     w.DtManyToManyJoinChildDataTypeKey != null))
        {
            DataType? oneRec = dataTypesList.SingleOrDefault(s => s.DtTable == dataTypeSeederModel.DtTable);
            if (oneRec == null)
            {
                continue;
            }

            oneRec.DtManyToManyJoinChildDataTypeId =
                tempData.GetIntIdByKey<DataType>(dataTypeSeederModel.DtManyToManyJoinChildDataTypeKey!);
            forUpdate.Add(oneRec);
        }

        return DataSeederRepo.SetUpdates(forUpdate);
    }

    protected virtual bool RemoveRedundantDataTypes()
    {
        string[] toRemoveTableNames = new[] { "dataRights", "dataRightTypes", "forms" };
        return CarcassRepo.RemoveRedundantDataTypesByTableNames(toRemoveTableNames);
    }

    protected virtual bool SetParentDataTypes()
    {
        var tempData = DataSeederTempData.Instance;
        string dataTypeTableName = UnitOfWork.GetTableName<DataType>();
        string crudRightTypeTableName = UnitOfWork.GetTableName<CrudRightType>();
        int dataTypeId = tempData.GetIntIdByKey<DataType>(dataTypeTableName);

        var dtdt = new Tuple<int, int>[]
        {
            new(tempData.GetIntIdByKey<DataType>(UnitOfWork.GetTableName<MenuItm>()),
                tempData.GetIntIdByKey<DataType>(UnitOfWork.GetTableName<MenuGroup>()))
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

    public override List<DataType> CreateListByRules()
    {
        //var appClaimDKey = ECarcassDataTypeKeys.AppClaim.ToDtKey();
        //var crudRightTypeDKey = ECarcassDataTypeKeys.CrudRightType.ToDtKey();
        DataType[] newDataTypes = new[]
        {
            //carcass used

            //AppClaim
            DataTypeFactory.Create("სპეციალური უფლებები", "სპეციალური უფლება", "სპეციალური უფლების",
                UnitOfWork.GetTableName<AppClaim>(), nameof(AppClaim.AclId).UnCapitalize(), null,
                nameof(AppClaim.AclKey).UnCapitalize(), null, nameof(AppClaim.AclName).UnCapitalize(), null),

            //DataType
            DataTypeFactory.Create("მონაცემთა ტიპები", "მონაცემთა ტიპი", "მონაცემთა ტიპის",
                UnitOfWork.GetTableName<DataType>(), nameof(DataType.DtId).UnCapitalize(), null,
                nameof(DataType.DtTable).UnCapitalize(), "ცხრილი", nameof(DataType.DtName).UnCapitalize(), null,
                GetTextBoxCell(nameof(DataType.DtNameNominative).UnCapitalize(), "სახელობითი"),
                GetTextBoxCell(nameof(DataType.DtNameGenitive).UnCapitalize(), "მიცემითი"),
                GetTextBoxCell(nameof(DataType.DtIdFieldName).UnCapitalize(), "იდენტიფიკატორი ველის სახელი"),
                GetTextBoxCell(nameof(DataType.DtKeyFieldName).UnCapitalize(), "კოდი ველის სახელი"),
                GetTextBoxCell(nameof(DataType.DtNameFieldName).UnCapitalize(), "სახელი ველის სახელი"),
                GetMdComboCell(nameof(DataType.DtParentDataTypeId).UnCapitalize(), "უფლებების მშობელი",
                    UnitOfWork.GetTableName<DataType>())),

            //dataTypeToCrudTypeModel
            DataTypeFactory.CreatePseudo("მონაცემების ცვლილებაზე უფლებები", "მონაცემების ცვლილებაზე უფლება",
                "მონაცემების ცვლილებაზე უფლების", UnitOfWork.GetTableName<DataType>(),
                UnitOfWork.GetTableName<CrudRightType>()),

            //CrudRightType
            DataTypeFactory.Create("მონაცემების ცვლილებაზე უფლებების ტიპები", "მონაცემების ცვლილებაზე უფლების ტიპი",
                "მონაცემების ცვლილებაზე უფლების ტიპის", UnitOfWork.GetTableName<CrudRightType>(),
                nameof(CrudRightType.CrtId).UnCapitalize(), null, nameof(CrudRightType.CrtKey).UnCapitalize(), null,
                nameof(CrudRightType.CrtName).UnCapitalize(), null),

            //DataTypeToDataTypeModel
            DataTypeFactory.CreatePseudo("უფლებები", "უფლება", "უფლების", UnitOfWork.GetTableName<DataType>(),
                UnitOfWork.GetTableName<DataType>()),

            //MenuGroup
            DataTypeFactory.Create("მენიუს ჯგუფები", "მენიუს ჯგუფი", "მენიუს ჯგუფის",
                UnitOfWork.GetTableName<MenuGroup>(), nameof(MenuGroup.MengId).UnCapitalize(), null,
                nameof(MenuGroup.MengKey).UnCapitalize(), null, nameof(MenuGroup.MengName).UnCapitalize(), null,
                GetSortIdCell(), GetTextBoxCell(nameof(MenuGroup.MengIconName).UnCapitalize(), "ხატულა")),

            //MenuItm
            DataTypeFactory.Create("მენიუ", "მენიუ", "მენიუს", UnitOfWork.GetTableName<MenuItm>(),
                nameof(MenuItm.MenId).UnCapitalize(), null, nameof(MenuItm.MenKey).UnCapitalize(), null,
                nameof(MenuItm.MenName).UnCapitalize(), null, GetSortIdCell(),
                GetTextBoxCell(nameof(MenuItm.MenValue).UnCapitalize(), "პარამეტრი"),
                GetMdComboCell(nameof(MenuItm.MenGroupId).UnCapitalize(), "ჯგუფი",
                    UnitOfWork.GetTableName<MenuGroup>()),
                GetTextBoxCell(nameof(MenuItm.MenLinkKey).UnCapitalize(), "ბმული"),
                GetTextBoxCell(nameof(MenuItm.MenIconName).UnCapitalize(), "ხატულა")),

            //Role
            DataTypeFactory.Create("როლები", "როლი", "როლის", UnitOfWork.GetTableName<Role>(),
                nameof(Role.RolId).UnCapitalize(), null, nameof(Role.RolKey).UnCapitalize(), null,
                nameof(Role.RolName).UnCapitalize(), null,
                GetIntegerCell(nameof(Role.RolLevel).UnCapitalize(), "დონე")),

            //User
            DataTypeFactory.Create("მომხმარებლები", "მომხმარებელი", "მომხმარებლის", UnitOfWork.GetTableName<User>(),
                nameof(User.UsrId).UnCapitalize(), null, nameof(User.NormalizedUserName).UnCapitalize(),
                "მომხმარებლის სახელი ნორმალიზებული", nameof(User.FullName).UnCapitalize(), "სრული სახელი",
                GetTextBoxCell(nameof(User.UserName).UnCapitalize(), "მომხმარებლის სახელი"),
                GetTextBoxCell(nameof(User.Email).UnCapitalize(), "ელექტრონული ფოსტის მისამართი"),
                GetTextBoxCell(nameof(User.FirstName).UnCapitalize(), "სახელი-"),
                GetTextBoxCell(nameof(User.LastName).UnCapitalize(), "გვარი"))
        };

        return [.. newDataTypes];
    }

    protected static Cell GetTextBoxCell(string fieldName, string caption, bool allowNull = false)
    {
        return allowNull
            ? Cell.CreateStringCell(fieldName, caption).Nullable().Default()
            : Cell.CreateStringCell(fieldName, caption).Required($"{caption} შევსებული უნდა იყოს").Default();
    }

    protected static Cell GetDatePickerCell(string fieldName, string caption, bool allowNull = false)
    {
        return allowNull
            ? Cell.Date(fieldName, caption).Nullable().Default()
            : Cell.Date(fieldName, caption).Required($"{caption} შევსებული უნდა იყოს").Default();
    }

    protected static Cell GetDateOnlyCell(string fieldName, string caption, bool allowNull = false)
    {
        return allowNull
            ? Cell.Date(fieldName, caption).Nullable().Default().DateOnly()
            : Cell.Date(fieldName, caption).Required($"{caption} შევსებული უნდა იყოს").Default().DateOnly();
    }

    //GetNumberColumn(3,"mrPosition","პოზიცია")
    protected static Cell GetIntegerCell(string fieldName, string caption, bool allowNull = false, bool isShort = false)
    {
        IntegerCell res = allowNull
            ? Cell.CreateIntegerCell(fieldName, caption).Nullable().Min(0)
            : Cell.CreateIntegerCell(fieldName, caption).Required($"{caption} შევსებული უნდა იყოს").Default();
        return isShort ? res.UseShortCell() : res;
    }

    protected static Cell GetSortIdCell()
    {
        return Cell.CreateIntegerCell("sortId", "რიგითი ნომერი").Required("რიგითი ნომერი შევსებული უნდა იყოს")
            .Default(-1).Min(-1).SortId();
    }

    protected static Cell GetMdComboCell(string fieldName, string caption, string dtTable, bool allowNull = false)
    {
        MdLookupCell cell = Cell.MdLookup(fieldName, caption, dtTable).Default();
        return allowNull
            ? cell.Nullable()
            : cell.Positive($"{caption} არჩეული უნდა იყოს").Required($"{caption} არჩეული უნდა იყოს");
    }

    protected static Cell GetComboCellWithRowSource(string fieldName, string caption, string rowSource,
        bool isShort = false)
    {
        RsLookupCell res = Cell.RsLookup(fieldName, caption, rowSource).Default();
        return isShort ? res.UseShortCell() : res;
    }

    protected static Cell GetCheckBoxCell(string fieldName, string caption)
    {
        return Cell.Boolean(fieldName, caption);
    }
}
