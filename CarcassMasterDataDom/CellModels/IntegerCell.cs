using System.Collections.Generic;
using CarcassMasterDataDom.Validation;
using Newtonsoft.Json;
using SystemToolsShared;

namespace CarcassMasterDataDom.CellModels;

public /*open*/ class IntegerCell : NumberCell
{
    public IntegerCell(string fieldName, string? caption, string? errorCode = null, string? errorMessage = null,
        bool visible = true, string? typeName = null) : base(fieldName, caption, visible,
        typeName ?? CellTypeNameForSave(nameof(IntegerCell)))
    {
        IsIntegerErr = visible
            ? new Err
            {
                ErrorCode = errorCode ?? $"{FieldName}MustBeInteger",
                ErrorMessage = errorMessage ?? $"{Caption} მთელი უნდა იყოს"
            }
            : null;
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? Def { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Err? IsIntegerErr { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public IntRule? MinValRule { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsShort { get; set; }

    public static IntegerCell Create(string fieldName, string? caption, string? errorCode = null,
        string? errorMessage = null, bool visible = true, string? typeName = null)
    {
        return new IntegerCell(fieldName, caption, errorCode, errorMessage, visible, typeName);
    }

    public new IntegerCell Required(string? errorCode = null, string? errorMessage = null)
    {
        base.Required(errorCode, errorMessage);
        return this;
    }

    public new IntegerCell Nullable(bool isNullable = true)
    {
        base.Nullable(isNullable);
        return this;
    }

    public IntegerCell Min(int minVal, string? errorCode = null, string? errorMessage = null)
    {
        MinValRule = new IntRule(minVal, errorCode ?? $"{FieldName}MinValueMustBe{minVal}",
            errorMessage ?? $"{Caption} ველის მინიმალური დასაშვები მნიშვნელობაა {minVal}");
        return this;
    }

    public IntegerCell Default(int defaultValue = default)
    {
        Def = defaultValue;
        return this;
    }

    public new IntegerCell Positive(string? errorCode = null, string? errorMessage = null)
    {
        base.Positive(errorCode, errorMessage);
        return this;
    }

    public override List<Err> Validate(object? value)
    {
        var errors = base.Validate(value);

        int testIntValue;
        if (IsShort)
        {
            errors = ValidateByType<short>(errors, value, "მოკლე მთელი");

            if (value is not short shortValue)
                return errors;

            testIntValue = shortValue;
        }
        else
        {
            errors = ValidateByType<int>(errors, value, "მთელი");

            if (value is not int intValue)
                return errors;

            testIntValue = intValue;
        }

        if (MinValRule is not null && testIntValue < MinValRule.Val)
            errors.Add(MinValRule.Err);

        if (IsPositiveErr is not null && testIntValue <= 0)
            errors.Add(IsPositiveErr.Value);

        return errors;
    }

    public IntegerCell Short()
    {
        IsShort = true;
        return this;
    }
}