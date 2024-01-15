using Newtonsoft.Json;

namespace CarcassMasterDataDom.CellModels;

public sealed class RsLookupCell : IntegerCell
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public RsLookupCell(string fieldName, string? caption, string rowSource, string? intErrCode = null,
        string? intErrMessage = null, bool visible = true, string? typeName = null) : base(fieldName, caption,
        intErrCode, intErrMessage, visible, typeName ?? CellTypeNameForSave(nameof(RsLookupCell)))
    {
        RowSource = rowSource;
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? RowSource { get; set; }

    public static RsLookupCell Create(string fieldName, string? caption, string rowSource, string? intErrCode = null,
        string? intErrMessage = null, bool visible = true, string? typeName = null)
    {
        return new RsLookupCell(fieldName, caption, rowSource, intErrCode, intErrMessage, visible, typeName);
    }

    public new RsLookupCell Default(int defaultValue = default)
    {
        base.Default(defaultValue);
        return this;
    }
}