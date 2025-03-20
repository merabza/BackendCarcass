using System.Collections.Generic;
using BackendCarcassContracts.Errors;
using Newtonsoft.Json;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom.CellModels;

public /*open*/ class MixedCell : Cell
{
    //public MixedCell()
    //{

    //}

    // ReSharper disable once ConvertToPrimaryConstructor
    public MixedCell(string fieldName, string? caption, bool visible = true, string? typeName = null) : base(
        typeName ?? CellTypeNameForSave(nameof(MixedCell)), fieldName, caption, visible)
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

    protected MixedCell Required(string? errorCode = null, string? errorMessage = null)
    {
        IsRequiredErr = CarcassMasterDataDomErrors.Required(FieldName, Caption, errorCode, errorMessage);
        return this;
    }

    protected MixedCell Nullable(bool isNullable = false)
    {
        IsNullable = true;
        return this;
    }

    public override List<Err> Validate(object? value)
    {
        List<Err> errors = [];
        if (IsRequiredErr is not null && value is null)
            errors.Add(IsRequiredErr.Value);

        return errors;
    }

    protected List<Err> ValidateByType<T>(List<Err> errors, object? value, string typeName)
    {
        if (value is T)
            return errors;

        errors.Add(CarcassMasterDataDomErrors.MustBeBoolean(FieldName, Caption, typeName));

        return errors;
    }
}