using LibCrud;

namespace CarcassMasterDataDom.Models;

public class MasterDataCrudData : ICrudData
{
    public readonly string Json;

    public MasterDataCrudData(string json)
    {
        Json = json;
    }
}