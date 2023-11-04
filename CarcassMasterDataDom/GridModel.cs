using System;
using System.Collections.Generic;
using CarcassMasterDataDom.CellModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarcassMasterDataDom;

public sealed class GridModel
{
    public List<Cell> Cells { get; set; } = new();


    public static GridModel? DeserializeGridModel(string dtGridRulesJson)
    {
        var jo = JObject.Parse(dtGridRulesJson);
        var jCommands = (JArray?)jo["cells"];
        if (jCommands == null)
            return null;
        GridModel gridModel = new();
        foreach (var jt in jCommands)
        {
            var cJson = jt.ToString();
            var strTypeName = jt["typeName"]?.Value<string>();
            switch (strTypeName)
            {
                case "Integer":
                {
                    var intCell = JsonConvert.DeserializeObject<IntegerCell>(cJson) ??
                                  throw new Exception("intCell is null");
                    gridModel.Cells.Add(intCell);
                    break;
                }
                case "Boolean":
                {
                    var boolCell = JsonConvert.DeserializeObject<BooleanCell>(cJson) ??
                                   throw new Exception("boolCell is null");
                    gridModel.Cells.Add(boolCell);
                    break;
                }
                case "Date":
                {
                    var dateCell = JsonConvert.DeserializeObject<DateCell>(cJson) ??
                                   throw new Exception("dateCell is null");
                    gridModel.Cells.Add(dateCell);
                    break;
                }
                case "String":
                {
                    var stringCell = JsonConvert.DeserializeObject<StringCell>(cJson) ??
                                     throw new Exception("stringCell is null");
                    gridModel.Cells.Add(stringCell);
                    break;
                }
            }
        }

        return gridModel;
    }
}