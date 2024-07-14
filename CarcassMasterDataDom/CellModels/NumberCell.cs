using BackendCarcassContracts.Errors;
using Newtonsoft.Json;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom.CellModels;

public /*open*/ class NumberCell : MixedCell
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public NumberCell(string fieldName, string? caption, bool visible = true, string? typeName = null) : base(fieldName,
        caption, visible, typeName ?? CellTypeNameForSave(nameof(NumberCell)))
    {
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Err? IsPositiveErr { get; set; }

    public new static NumberCell Create(string fieldName, string? caption, bool visible = true, string? typeName = null)
    {
        return new NumberCell(fieldName, caption, visible, typeName);
    }

    protected new NumberCell Required(string? errorCode = null, string? errorMessage = null)
    {
        base.Required(errorCode, errorMessage);
        return this;
    }

    protected new NumberCell Nullable(bool isNullable = true)
    {
        base.Nullable(isNullable);
        return this;
    }

    protected NumberCell Positive(string? errorCode = null, string? errorMessage = null)
    {
        IsPositiveErr = CarcassMasterDataDomErrors.MustBePositive(FieldName, Caption, errorCode, errorMessage);
        return this;
    }
}