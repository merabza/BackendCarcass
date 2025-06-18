using System.Collections.Generic;
using CarcassDb.Models;
using CarcassMasterDataDom;
using CarcassMasterDataDom.CellModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarcassDb.Factorys;

public static class DataTypeFactory
{
    public static DataType CreatePseudo(string dtName, string dtNameNominative, string dtNameGenitive,
        string table1Name, string table2Name)
    {
        return new DataType
        {
            DtName = dtName,
            DtNameNominative = dtNameNominative,
            DtNameGenitive = dtNameGenitive,
            DtTable = $"{table1Name}{table2Name}"
        };
    }

    public static DataType Create(string dtName, string dtNameNominative, string dtNameGenitive, string tableName,
        string idFieldName, string? keyFieldName, string? nameFieldName, params List<Cell> additionalCells)
    {
        var serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var gridModel = new GridModel
        {
            Cells =
            [
                GetAutoNumberColumn(idFieldName)
            ]
        };

        if (!string.IsNullOrEmpty(keyFieldName))
            gridModel.Cells.Add(GetKeyColumn(keyFieldName));

        if (!string.IsNullOrEmpty(nameFieldName))
            gridModel.Cells.Add(GetNameColumn(nameFieldName));

        if (additionalCells.Count > 0)
            gridModel.Cells.AddRange(additionalCells);

        return new DataType
        {
            DtName = dtName,
            DtNameNominative = dtNameNominative,
            DtNameGenitive = dtNameGenitive,
            DtTable = tableName,
            DtIdFieldName = idFieldName,
            DtKeyFieldName = keyFieldName,
            DtNameFieldName = nameFieldName,
            DtGridRulesJson = JsonConvert.SerializeObject(gridModel, serializerSettings)
        };
    }

    private static IntegerCell GetAutoNumberColumn(string fieldName)
    {
        return Cell.Integer(fieldName, null, "", "", false).Default();
    }

    private static StringCell GetKeyColumn(string fieldName)
    {
        return GetTextBoxCell(fieldName, "კოდი");
    }

    private static StringCell GetNameColumn(string fieldName)
    {
        return GetTextBoxCell(fieldName, "სახელი");
    }

    private static StringCell GetTextBoxCell(string fieldName, string caption, bool allowNull = false)
    {
        return allowNull
            ? Cell.String(fieldName, caption).Nullable().Default()
            : Cell.String(fieldName, caption).Required($"{caption} შევსებული უნდა იყოს").Default();
    }
}