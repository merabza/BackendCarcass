namespace CarcassContracts.V1.Requests;

public sealed class LoginRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}