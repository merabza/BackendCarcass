using Newtonsoft.Json;
using SystemToolsShared;

namespace CarcassMasterDataDom.CellModels;

public /*open*/ class NumberCell : MixedCell
{
    public NumberCell(string fieldName, string? caption, bool visible = true, string? typeName = null) : base(fieldName,
        caption, visible, typeName ?? "Number")
    {
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Err? IsPositiveErr { get; set; }

    public new static NumberCell Create(string fieldName, string? caption, bool visible = true, string? typeName = null)
    {
        return new NumberCell(fieldName, caption, visible, typeName);
    }

    public new NumberCell Required(string? errorCode = null, string? errorMessage = null)
    {
        base.Required(errorCode, errorMessage);
        return this;
    }

    public new NumberCell Nullable(bool isNullable = true)
    {
        base.Nullable(isNullable);
        return this;
    }

    public NumberCell Positive(string? errorCode = null, string? errorMessage = null)
    {
        IsPositiveErr = new Err
        {
            ErrorCode = errorCode ?? $"{FieldName}MustBePositive",
            ErrorMessage = errorMessage ?? $"{Caption} უნდა იყოს დადებითი რიცხვი"
        };
        return this;
    }
}