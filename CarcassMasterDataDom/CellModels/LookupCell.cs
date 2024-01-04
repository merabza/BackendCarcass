using Newtonsoft.Json;

namespace CarcassMasterDataDom.CellModels;

public sealed class LookupCell(
    string fieldName,
    string? caption,
    string dataMember,
    string valueMember,
    string displayMember,
    string? intErrCode = null,
    string? intErrMessage = null,
    bool visible = true,
    string? typeName = null) : IntegerCell(fieldName, caption, intErrCode, intErrMessage, visible, typeName ?? "Lookup")
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? DataMember { get; set; } = dataMember;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ValueMember { get; set; } = valueMember;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? DisplayMember { get; set; } = displayMember;

    public static LookupCell Create(string fieldName, string? caption, string dataMember, string valueMember,
        string displayMember, string? intErrCode = null, string? intErrMessage = null, bool visible = true,
        string? typeName = null)
    {
        return new LookupCell(fieldName, caption, dataMember, valueMember, displayMember, intErrCode, intErrMessage,
            visible, typeName);
    }

    public new LookupCell Default(int defaultValue = default)
    {
        base.Default(defaultValue);
        return this;
    }
}