using System.Collections.Generic;
using CarcassMasterDataDom.Models;

namespace CarcassDom.Models;

public sealed class DataTypeModel
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypeModel(int dtId, string dtTable, string dtName, int? dtParentDataTypeId)
    {
        DtId = dtId;
        DtTable = dtTable;
        DtName = dtName;
        DtParentDataTypeId = dtParentDataTypeId;
    }

    public int DtId { get; set; }
    public string DtName { get; set; }
    public string DtTable { get; set; }

    public int? DtParentDataTypeId { get; set; }

    //[NotMapped] public List<Err> Errors { get; set; } = new();
    public List<ReturnValueModel> ReturnValues { get; set; } = [];
}