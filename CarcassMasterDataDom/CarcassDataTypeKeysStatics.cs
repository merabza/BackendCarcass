namespace CarcassMasterDataDom;

public static class CarcassDataTypeKeysStatics
{
    public static string ToDtKey(this ECarcassDataTypeKeys carcassDataTypeKey)
    {
        return CarcassDataTypeKeys.Instance.GetDtKey(carcassDataTypeKey);
    }
}