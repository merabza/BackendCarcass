namespace BackendCarcass.DataSeeding.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MenuItmSeederModel
{
    public required string MenGroupIdMengKey { get; set; }
    public string? MenIconName { get; set; }
    public required string MenKey { get; set; }
    public required string MenLinkKey { get; set; }
    public required string MenName { get; set; }
    public string? MenValue { get; set; }
    public int SortId { get; set; }
}
