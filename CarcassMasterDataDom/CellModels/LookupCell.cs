using Newtonsoft.Json;

namespace CarcassMasterDataDom.CellModels;

public sealed class LookupCell : IntegerCell
{
    //public LookupCell(string fieldName, string? caption, string rowSource, string? intErrCode = null,
    //    string? intErrMessage = null, bool visible = true, string? typeName = null) : base(fieldName, caption,
    //    intErrCode, intErrMessage, visible, typeName ?? "Lookup")
    //{
    //    RowSource = rowSource;
    //}

    public LookupCell(string fieldName, string? caption, string dataMember, string valueMember, string displayMember,
        string? intErrCode = null, string? intErrMessage = null, bool visible = true, string? typeName = null) : base(
        fieldName, caption, intErrCode, intErrMessage, visible, typeName ?? "Lookup")
    {
        DataMember = dataMember;
        ValueMember = valueMember;
        DisplayMember = displayMember;
    }

    //[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    //public string? RowSource { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? DataMember { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? ValueMember { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? DisplayMember { get; set; }

    //public static LookupCell Create(string fieldName, string? caption, string rowSource, string? intErrCode = null,
    //    string? intErrMessage = null, bool visible = true, string? typeName = null)
    //{
    //    return new LookupCell(fieldName, caption, rowSource, intErrCode, intErrMessage, visible, typeName);
    //}

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