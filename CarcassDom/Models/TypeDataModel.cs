namespace CarcassDom.Models;

public sealed class TypeDataModel
{
    public TypeDataModel(int dtId, string dKey)
    {
        DtId = dtId;
        DKey = dKey;
    }

    public int DtId { get; set; }
    public string DKey { get; set; }
}