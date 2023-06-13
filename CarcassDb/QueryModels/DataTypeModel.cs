using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SystemToolsShared;

namespace CarcassDb.QueryModels;

public sealed class DataTypeModel
{
    public DataTypeModel(int dtId, string dtKey, string dtName, string dtTable, int? dtParentDataTypeId)
    {
        DtId = dtId;
        DtKey = dtKey;
        DtName = dtName;
        DtTable = dtTable;
        DtParentDataTypeId = dtParentDataTypeId;
    }

    public int DtId { get; set; }
    public string DtKey { get; set; }
    public string DtName { get; set; }
    public string DtTable { get; set; }
    public int? DtParentDataTypeId { get; set; }
    [NotMapped] public List<Err> Errors { get; set; } = new();
    [NotMapped] public List<ReturnValueModel> ReturnValues { get; set; } = new();
}