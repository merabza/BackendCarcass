namespace BackendCarcass.DataSeeding.Models;

public sealed class ManyToManyJoinSeederModel
{
    public required string PtIdDtKey { get; set; }
    public required string PKey { get; set; }
    public required string CtIdDtKey { get; set; }
    public required string CKey { get; set; }
}