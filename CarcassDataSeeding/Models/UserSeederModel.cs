namespace CarcassDataSeeding.Models;

public sealed class UserSeederModel
{
    public string FirstName { get; set; }
    public string FullName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public string UserName { get; set; }
    public string NormalizedUserName { get; set; }
    public string Email { get; set; }
    public string NormalizedEmail { get; set; }
}