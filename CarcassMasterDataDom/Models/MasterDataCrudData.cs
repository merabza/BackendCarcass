using LibCrud;

namespace CarcassMasterDataDom.Models;

public class MasterDataCrudData : ICrudData
{
    public readonly string Json;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataCrudData(string json)
    {
        Json = json;
    }
}