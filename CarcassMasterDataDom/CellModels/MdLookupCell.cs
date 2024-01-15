using Newtonsoft.Json;

namespace CarcassMasterDataDom.CellModels;

public sealed class MdLookupCell : IntegerCell
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdLookupCell(string fieldName, string? caption, string dtTable, string? intErrCode = null,
        string? intErrMessage = null, bool visible = true, string? typeName = null) : base(fieldName, caption,
        intErrCode, intErrMessage, visible, typeName ?? CellTypeNameForSave(nameof(MdLookupCell)))
    {
        DtTable = dtTable;
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? DtTable { get; set; }

    public static MdLookupCell Create(string fieldName, string? caption, string dtTable, string? intErrCode = null,
        string? intErrMessage = null, bool visible = true, string? typeName = null)
    {
        return new MdLookupCell(fieldName, caption, dtTable, intErrCode, intErrMessage, visible, typeName);
    }

    public new MdLookupCell Default(int defaultValue = default)
    {
        base.Default(defaultValue);
        return this;
    }
}