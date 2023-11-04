using System.ComponentModel.DataAnnotations.Schema;
using CarcassMasterDataDom;

namespace CarcassDb.QueryModels;

//უფლების მოდელი
public sealed class DataTypeToDataTypeModel : IDataType
{

    public DataTypeToDataTypeModel(int dtdtId, string dtdtKey, string dtdtName, string pKey)
    {
        DtdtId = dtdtId;
        DtdtKey = dtdtKey;
        DtdtName = dtdtName;
        PKey = pKey;
    }

    public int DtdtId { get; set; }
    public string DtdtKey { get; set; }
    public string DtdtName { get; set; }
    public string PKey { get; set; }

    [NotMapped]
    public int Id
    {
        get => DtdtId;
        set => DtdtId = value;
    }

    [NotMapped] public string Key => DtdtKey;

    [NotMapped] public string Name => DtdtName;

    [NotMapped] public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        if (data is not DataTypeToDataTypeModel newData)
            return false;
        DtdtKey = newData.DtdtKey;
        DtdtName = newData.DtdtName;
        PKey = newData.PKey;
        return true;
    }

    public dynamic EditFields()
    {
        return new { MmjId = DtdtId, MmjKey = DtdtKey, MmjName = DtdtName, PKey };
    }
}