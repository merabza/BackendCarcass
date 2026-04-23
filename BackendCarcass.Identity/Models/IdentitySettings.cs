namespace BackendCarcass.Identity.Models;

public sealed class IdentitySettings
{
    public string? JwtSecret { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtAudience { get; set; }
}
