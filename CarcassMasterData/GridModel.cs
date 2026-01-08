using System;
using System.Collections.Generic;
using CarcassMasterData.CellModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarcassMasterData;

public sealed class GridModel
{
    public List<Cell> Cells { get; set; } = [];

    public static GridModel? DeserializeGridModel(string dtGridRulesJson)
    {
        var jo = JObject.Parse(dtGridRulesJson);
        var jCommands = (JArray?)jo["cells"];
        if (jCommands is null)
            return null;
        var gridModel = new GridModel();
        foreach (var jt in jCommands)
        {
            var cJson = jt.ToString();
            var strTypeName = jt["typeName"]?.Value<string>();
            if (strTypeName is null)
                continue;

            var cellTypeName = $"{strTypeName}{nameof(Cell)}";
            var method = typeof(JsonConvert).GetMethod(nameof(JsonConvert.DeserializeObject), 1, [typeof(string)]);
            var genType = Type.GetType($"{nameof(CarcassMasterData)}.{nameof(CellModels)}.{cellTypeName}");
            if (genType is null)
                continue;
            var generic = method?.MakeGenericMethod(genType);
            var cellObject = generic?.Invoke(null, [cJson]) ?? throw new Exception($"{cellTypeName} is null");
            gridModel.Cells.Add((Cell)cellObject);

            //if (!Enum.TryParse<ECellTypeName>(strTypeName, out var cellType))
            //    continue;
            ////შეიძლება ისე მოფიქრება, რომ აქ switch არ იყოს
            //switch (cellType)
            //{
            //    case ECellTypeName.Integer:
            //        var integerCell = JsonConvert.DeserializeObject<IntegerCell>(cJson) ??
            //                          throw new Exception($"{nameof(IntegerCell)} is null");
            //        gridModel.Cells.Add(integerCell);
            //        break;
            //    case ECellTypeName.Boolean:
            //        var boolCell = JsonConvert.DeserializeObject<BooleanCell>(cJson) ??
            //                       throw new Exception($"{nameof(BooleanCell)} is null");
            //        gridModel.Cells.Add(boolCell);
            //        break;
            //    case ECellTypeName.Date:
            //        var dateCell = JsonConvert.DeserializeObject<DateCell>(cJson) ??
            //                       throw new Exception($"{nameof(DateCell)} is null");
            //        gridModel.Cells.Add(dateCell);
            //        break;
            //    case ECellTypeName.String:
            //        var stringCell = JsonConvert.DeserializeObject<StringCell>(cJson) ??
            //                         throw new Exception($"{nameof(StringCell)} is null");
            //        gridModel.Cells.Add(stringCell);
            //        break;
            //    case ECellTypeName.MdLookup:
            //        var mdLookupCell = JsonConvert.DeserializeObject<MdLookupCell>(cJson) ??
            //                           throw new Exception($"{nameof(MdLookupCell)} is null");
            //        gridModel.Cells.Add(mdLookupCell);
            //        break;
            //    case ECellTypeName.Mixed:
            //        var mixedCell = JsonConvert.DeserializeObject<MixedCell>(cJson) ??
            //                        throw new Exception($"{nameof(MixedCell)} is null");
            //        gridModel.Cells.Add(mixedCell);
            //        break;
            //    case ECellTypeName.Number:
            //        var numberCell = JsonConvert.DeserializeObject<NumberCell>(cJson) ??
            //                         throw new Exception($"{nameof(NumberCell)} is null");
            //        gridModel.Cells.Add(numberCell);
            //        break;
            //    case ECellTypeName.Lookup: //deprecated
            //        var lookupCell = JsonConvert.DeserializeObject<LookupCell>(cJson) ??
            //                         throw new Exception($"{nameof(LookupCell)} is null");
            //        gridModel.Cells.Add(lookupCell);
            //        break;
            //    case ECellTypeName.RsLookup:
            //        var rsLookupCell = JsonConvert.DeserializeObject<RsLookupCell>(cJson) ??
            //                           throw new Exception($"{nameof(RsLookupCell)} is null");
            //        gridModel.Cells.Add(rsLookupCell);
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
        }

        return gridModel;
    }
}