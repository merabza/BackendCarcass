using BackendCarcass.Application.Crud;

namespace BackendCarcass.Application.MasterData.Models;

public sealed class MasterDataCrudLoadedData : ICrudData
{
    public MasterDataCrudLoadedData(dynamic entry)
    {
        Entry = entry;
    }

    public dynamic Entry { get; set; }
}
