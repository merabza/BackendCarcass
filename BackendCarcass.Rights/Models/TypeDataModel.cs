namespace BackendCarcass.Rights.Models;

public sealed class TypeDataModel
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public TypeDataModel(int dtId, string dKey)
    {
        DtId = dtId;
        DKey = dKey;
    }

    public int DtId { get; set; }
    public string DKey { get; set; }
}
