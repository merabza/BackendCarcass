using LibCrud;

namespace CarcassMasterDataDom.Models;

public sealed class MasterDataCrudLoadedData : ICrudData
{
    public MasterDataCrudLoadedData(dynamic entry)
    {
        Entry = entry;
    }

    public dynamic Entry { get; set; }
}