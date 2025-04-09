using LibCrud;

namespace CarcassMasterDataDom.Models;

public sealed class MasterDataCrudData : ICrudData
{
    public readonly string Json;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataCrudData(string json)
    {
        Json = json;
    }
}