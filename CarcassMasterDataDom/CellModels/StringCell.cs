using System.Collections.Generic;
using CarcassContracts.Errors;
using CarcassMasterDataDom.Validation;
using Newtonsoft.Json;
using SystemToolsShared;

namespace CarcassMasterDataDom.CellModels;

public sealed class StringCell : MixedCell
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public StringCell(string fieldName, string? caption, bool visible = true, string? typeName = null) : base(fieldName,
        caption, visible, typeName ?? CellTypeNameForSave(nameof(StringCell)))
    {
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Def { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public IntRule? MaxLenRule { get; set; }

    public new static StringCell Create(string fieldName, string? caption, bool visible = true, string? typeName = null)
    {
        return new StringCell(fieldName, caption, visible, typeName);
    }

    public StringCell Default(string defaultValue = "")
    {
        Def = defaultValue;
        return this;
    }

    public new StringCell Required(string? errorCode = null, string? errorMessage = null)
    {
        base.Required(errorCode, errorMessage);
        return this;
    }

    public StringCell Max(int maxLen, string? errorCode = null, string? errorMessage = null)
    {
        MaxLenRule = new IntRule(maxLen, errorCode ?? $"{FieldName}TooLong",
            errorMessage ?? $"{Caption} ძალიან გრძელია");
        return this;
    }

    public new StringCell Nullable(bool isNullable = true)
    {
        base.Nullable(isNullable);
        return this;
    }

    public override List<Err> Validate(object? value)
    {
        var errMes = ValidateByType<string>(base.Validate(value), value, "სტრიქონის");

        if (value is not string strValue)
            return errMes;

        if (IsRequiredErr is not null && strValue == string.Empty)
            errMes.Add(CarcassMasterDataDomErrors.IsEmpty(FieldName, Caption));

        if (MaxLenRule is not null && strValue.Length > MaxLenRule.Val)
            errMes.Add(CarcassMasterDataDomErrors.IsTooLong(FieldName, Caption));

        return errMes;
    }
}