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
        var jCommands = (JArray?)jo["Cells"];
        if (jCommands == null)
            return null;
        GridModel gridModel = new();
        foreach (var jt in jCommands)
        {
            var cJson = jt.ToString();
            var strTypeName = jt["TypeName"]?.Value<string>();
            switch (strTypeName)
            {
                case "Integer":
                {
                    var intCell = JsonConvert.DeserializeObject<IntegerCell>(cJson);
                    if (intCell is null)
                        throw new Exception("intCell is null");
                    gridModel.Cells.Add(intCell);
                    break;
                }
                case "Boolean":
                {
                    var boolCell = JsonConvert.DeserializeObject<BooleanCell>(cJson);
                    if (boolCell is null)
                        throw new Exception("boolCell is null");
                    gridModel.Cells.Add(boolCell);
                    break;
                }
                case "Date":
                {
                    var dateCell = JsonConvert.DeserializeObject<DateCell>(cJson);
                    if (dateCell is null)
                        throw new Exception("dateCell is null");
                    gridModel.Cells.Add(dateCell);
                    break;
                }
                case "String":
                {
                    var stringCell = JsonConvert.DeserializeObject<StringCell>(cJson);
                    if (stringCell is null)
                        throw new Exception("stringCell is null");
                    gridModel.Cells.Add(stringCell);
                    break;
                }
            }
        }

        return gridModel;
    }
}