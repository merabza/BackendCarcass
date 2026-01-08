namespace CarcassMasterData.Models;

public sealed class ReturnValueModel
{
    public int Id { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }
    public int? ParentId { get; set; }
}