namespace CarcassDataSeeding.Models;

public sealed class RoleModel
{
    public required string RoleKey { get; set; }
    public required string RoleName { get; set; }
    public int Level { get; set; }
}