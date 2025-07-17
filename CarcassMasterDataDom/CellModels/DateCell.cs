using System;
using System.Collections.Generic;
using CarcassMasterDataDom.Validation;
using Newtonsoft.Json;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom.CellModels;

public sealed class DateCell : MixedCell
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DateCell(string fieldName, string? caption, bool showDate = true, bool showTime = true, bool visible = true,
        string? typeName = null) : base(fieldName, caption, visible, typeName ?? CellTypeNameForSave(nameof(DateCell)))
    {
        ShowDate = showDate;
        ShowTime = showTime;
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime? Def { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateRule? MinValRule { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool ShowDate { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool ShowTime { get; set; }

    public static DateCell Create(string fieldName, string? caption, bool showDate = true, bool showTime = true,
        bool visible = true, string? typeName = null)
    {
        return new DateCell(fieldName, caption, showDate, showTime, visible, typeName);
    }

    public new DateCell Required(string? errorCode = null, string? errorMessage = null)
    {
        base.Required(errorCode, errorMessage);
        return this;
    }

    public new DateCell Nullable(bool isNullable = true)
    {
        base.Nullable(isNullable);
        return this;
    }

    public DateCell Min(DateTime minVal, string? errorCode = null, string? errorMessage = null)
    {
        MinValRule = new DateRule(minVal, errorCode ?? $"{FieldName}MinValueMustBe{minVal}",
            errorMessage ?? $"{Caption} ველის მინიმალური დასაშვები მნიშვნელობაა {minVal}");
        return this;
    }

    public DateCell Default(DateTime defaultValue = default)
    {
        Def = defaultValue;
        return this;
    }

    public override List<Err> Validate(object? value)
    {
        var errors = ValidateByType<DateTime>(base.Validate(value), value, "თარიღის");

        if (value is not DateTime dateTimeValue)
            return errors;

        if (MinValRule is not null && dateTimeValue < MinValRule.Val)
            errors.Add(MinValRule.Err);

        return errors;
    }

    public DateCell DateOnly()
    {
        ShowDate = true;
        ShowTime = false;
        return this;
    }

    public DateCell TimeOnly()
    {
        ShowDate = false;
        ShowTime = true;
        return this;
    }
}