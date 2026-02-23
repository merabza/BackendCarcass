using System.Collections.Generic;
using BackendCarcass.MasterData.Validation;
using BackendCarcassShared.BackendCarcassContracts.Errors;
using Newtonsoft.Json;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData.CellModels;

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

    public static new StringCell Create(string fieldName, string? caption, bool visible = true, string? typeName = null)
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
        List<Err> errMes = ValidateByType<string>(base.Validate(value), value, "სტრიქონის");

        if (value is not string strValue)
        {
            return errMes;
        }

        if (IsRequiredErr is not null && string.IsNullOrEmpty(strValue))
        {
            errMes.Add(CarcassMasterDataErrors.IsEmpty(FieldName, Caption));
        }

        if (MaxLenRule is not null && strValue.Length > MaxLenRule.Val)
        {
            errMes.Add(CarcassMasterDataErrors.IsTooLong(FieldName, Caption));
        }

        return errMes;
    }
}
