using Newtonsoft.Json;

namespace BackendCarcass.MasterData.CellModels;

//Deprecated
public sealed class LookupCell : IntegerCell
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public LookupCell(string fieldName, string? caption, string dataMember, string valueMember, string displayMember,
        string? intErrCode = null, string? intErrMessage = null, bool visible = true, string? typeName = null) : base(
        fieldName, caption, intErrCode, intErrMessage, visible, typeName ?? CellTypeNameForSave(nameof(LookupCell)))
    {
        DataMember = dataMember;
        ValueMember = valueMember;
        DisplayMember = displayMember;
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? DataMember { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ValueMember { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? DisplayMember { get; set; }

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