using LibCrud;

namespace CarcassMasterData.Models;

public sealed class MasterDataCrudData : ICrudData
{
    public readonly string Json;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MasterDataCrudData(string json)
    {
        Json = json;
    }
}