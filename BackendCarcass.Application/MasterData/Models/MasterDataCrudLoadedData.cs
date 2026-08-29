using BackendCarcass.Application.Crud;

namespace BackendCarcass.Application.MasterData.Models;

public sealed class MasterDataCrudLoadedData : ICrudData
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataCrudLoadedData(dynamic entry)
    {
        Entry = entry;
    }

    public dynamic Entry { get; set; }
}
