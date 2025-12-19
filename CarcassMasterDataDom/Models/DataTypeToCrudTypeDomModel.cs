using System;
using SystemToolsShared;

namespace CarcassMasterDataDom.Models;

public sealed class DataTypeToCrudTypeDomModel : IDataType
{
    //public static string DtKeyKey => nameof(DtctId).CountDtKey();

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypeToCrudTypeDomModel(int dtctId, string dtctKey, string dtctName, int dataTypeId)
    {
        DtctId = dtctId;
        DtctKey = dtctKey;
        DtctName = dtctName;
        DataTypeId = dataTypeId;
    }

    public int DtctId { get; set; }
    public string DtctKey { get; init; }
    public string DtctName { get; init; }
    public int DataTypeId { get; init; }

    public int Id
    {
        get => DtctId;
        set => DtctId = value;
    }

    public string Key => DtctKey;
    public string Name => DtctName;
    public int? ParentId => null;

    public bool UpdateTo(IDataType data)
    {
        throw new NotImplementedException();
    }

    public dynamic EditFields()
    {
        return new DataTypeToCrudTypeDomModel(DtctId, DtctKey, DtctName, DataTypeId);
    }
}