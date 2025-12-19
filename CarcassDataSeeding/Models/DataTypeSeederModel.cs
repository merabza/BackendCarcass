namespace CarcassDataSeeding.Models;

public sealed class DataTypeSeederModel
{
    public string? DtParentDataTypeIdDtKey { get; set; }
    public string? DtManyToManyJoinParentDataTypeKey { get; set; }
    public string? DtManyToManyJoinChildDataTypeKey { get; set; }
    public string? DtGridRulesJson { get; set; }
    public string? DtIdFieldName { get; set; }
    public required string DtKey { get; set; }
    public required string DtTable { get; set; }
    public string? DtKeyFieldName { get; set; }
    public required string DtName { get; set; }
    public string? DtNameFieldName { get; set; }
    public required string DtNameGenitive { get; set; }
    public required string DtNameNominative { get; set; }
}