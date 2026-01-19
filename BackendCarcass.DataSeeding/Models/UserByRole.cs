namespace CarcassDataSeeding.Models;

public sealed class UserByRole
{
    public required string UserName { get; set; }
    public required string RoleName { get; set; }
}