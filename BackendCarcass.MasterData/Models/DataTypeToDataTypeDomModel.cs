using System;

namespace BackendCarcass.MasterData.Models;

public sealed class DataTypeToDataTypeDomModel : IDataType
{
    //public static string DtKeyKey => nameof(DtdtId).CountDtKey();

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypeToDataTypeDomModel(int dtdtId, string dtdtKey, string dtdtName, string pKey)
    {
        DtdtId = dtdtId;
        DtdtKey = dtdtKey;
        DtdtName = dtdtName;
        PKey = pKey;
    }

    public int DtdtId { get; set; }
    public string DtdtKey { get; init; }
    public string DtdtName { get; init; }
    public string PKey { get; init; }

    public int Id
    {
        get => DtdtId;
        set => DtdtId = value;
    }

    public string Key => DtdtKey;
    public string Name => DtdtName;
    public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        throw new NotImplementedException();
    }

    public dynamic EditFields()
    {
        return new DataTypeToDataTypeDomModel(DtdtId, DtdtKey, DtdtName, PKey);
    }
}