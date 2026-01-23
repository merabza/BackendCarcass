namespace BackendCarcass.DataSeeding.Models;

public sealed class RoleSeederModel
{
    public required string RolKey { get; set; }
    public int RolLevel { get; set; }
    public required string RolName { get; set; }
    public required string RolNormalizedKey { get; set; }
}