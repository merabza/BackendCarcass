using System.Collections.Generic;
using Newtonsoft.Json;
using SystemTools.SystemToolsShared.Errors;

namespace CarcassMasterData.CellModels;

public sealed class BooleanCell : MixedCell
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public BooleanCell(string fieldName, string? caption, bool visible = true, string? typeName = null) : base(
        fieldName, caption, visible, typeName ?? CellTypeNameForSave(nameof(BooleanCell)))
    {
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? Def { get; set; }

    public static new BooleanCell Create(string fieldName, string? caption, bool visible = true,
        string? typeName = null)
    {
        return new BooleanCell(fieldName, caption, visible, typeName);
    }

    public BooleanCell Default(bool? defaultValue = default)
    {
        Def = defaultValue;
        return this;
    }

    public override List<Err> Validate(object? value)
    {
        return ValidateByType<bool>(base.Validate(value), value, "ლოგიკური");
    }
}
