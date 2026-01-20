namespace BackendCarcass.Rights.Models;

public sealed class RightsChangeModel
{
    public TypeDataModel? Parent { get; set; }
    public TypeDataModel? Child { get; set; }
    public bool Checked { get; set; }
}
