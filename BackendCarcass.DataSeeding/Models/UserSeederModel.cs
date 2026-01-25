namespace BackendCarcass.DataSeeding.Models;

public sealed class UserSeederModel
{
    public required string FirstName { get; set; }
    public required string FullName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }
    public required string UserName { get; set; }
    public required string NormalizedUserName { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
}
