using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SystemToolsShared;

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

    public bool ShowDate { get; set; }
    public bool ShowTime { get; set; }

    public static DateCell Create(string fieldName, string? caption, bool showDate = true, bool showTime = true,
        bool visible = true, string? typeName = null)
    {
        return new DateCell(fieldName, caption, showDate, showTime, visible, typeName);
    }

    public DateCell Default(DateTime defaultValue = default)
    {
        Def = defaultValue;
        return this;
    }

    public override List<Err> Validate(object? value)
    {
        return ValidateByType<DateTime>(base.Validate(value), value, "თარიღის");
    }
}