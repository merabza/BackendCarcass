using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.QueryModels;

public sealed class DataTypeToCrudTypeModel : IDataType
{
    public DataTypeToCrudTypeModel(int dtctId, string dtctKey, string dtctName, int dataTypeId)
    {
        DtctId = dtctId;
        DtctKey = dtctKey;
        DtctName = dtctName;
        DataTypeId = dataTypeId;
    }

    public int DtctId { get; set; }
    public string DtctKey { get; set; }
    public string DtctName { get; set; }
    public int DataTypeId { get; set; }

    [NotMapped]
    public int Id
    {
        get => DtctId;
        set => DtctId = value;
    }

    [NotMapped] public string Key => DtctKey;

    [NotMapped] public string Name => DtctName;

    [NotMapped] public int? ParentId => DataTypeId;

    public bool UpdateTo(IDataType data)
    {
        if (data is not DataTypeToCrudTypeModel newData)
            return false;
        DtctKey = newData.DtctKey;
        DtctName = newData.DtctName;
        return true;
    }

    public dynamic EditFields()
    {
        return new { DtctId, DtctKey, DtctName };
    }
}