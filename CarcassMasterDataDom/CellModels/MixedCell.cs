using System.Collections.Generic;
using Newtonsoft.Json;
using SystemToolsShared;

namespace CarcassMasterDataDom.CellModels;

public /*open*/ class MixedCell : Cell
{
    //public MixedCell()
    //{

    //}

    public MixedCell(string fieldName, string? caption, bool visible = true, string? typeName = null) : base(
        typeName ?? "Mixed", fieldName, caption, visible)
    {
    }


    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Err? IsRequiredErr { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsNullable { get; set; }

    public static MixedCell Create(string fieldName, string caption, bool visible = true, string? typeName = null)
    {
        return new MixedCell(fieldName, caption, visible, typeName);
    }

    public MixedCell Required(string? errorCode = null, string? errorMessage = null)
    {
        IsRequiredErr = new Err
        {
            ErrorCode = errorCode ?? $"{FieldName}Required",
            ErrorMessage = errorMessage ?? $"{Caption} შევსებული უნდა იყოს"
        };
        return this;
    }

    public MixedCell Nullable(bool isNullable = false)
    {
        IsNullable = true;
        return this;
    }

    public override List<Err> Validate(object? value)
    {
        List<Err> errors = new();
        if (IsRequiredErr is not null && value == null)
            errors.Add(IsRequiredErr.Value);

        return errors;
    }


    protected List<Err> ValidateByType<T>(List<Err> errors, object? value, string typeName)
    {
        if (value is T)
            return errors;

        errors.Add(new Err
            { ErrorCode = $"{FieldName}MustBeBoolean", ErrorMessage = $"{Caption} ველი უნდა იყოს {typeName} ტიპის" });

        return errors;
    }
}