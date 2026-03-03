namespace BackendCarcass.Identity.Models;

public sealed class IdentitySettings
{
    public string? JwtSecret { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
}
