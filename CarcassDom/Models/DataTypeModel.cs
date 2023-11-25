using System.Collections.Generic;
using CarcassMasterDataDom.Models;

namespace CarcassDom.Models;

public sealed class DataTypeModel(int dtId, string dtKey, string dtName, string dtTable, int? dtParentDataTypeId)
{
    public int DtId { get; set; } = dtId;
    public string DtKey { get; set; } = dtKey;
    public string DtName { get; set; } = dtName;
    public string DtTable { get; set; } = dtTable;
    public int? DtParentDataTypeId { get; set; } = dtParentDataTypeId;
    //[NotMapped] public List<Err> Errors { get; set; } = new();
    public List<ReturnValueModel> ReturnValues { get; set; } = new();
}