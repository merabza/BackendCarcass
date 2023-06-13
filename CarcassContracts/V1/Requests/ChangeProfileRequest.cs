namespace CarcassContracts.V1.Requests;

public sealed class ChangeProfileRequest
{
    public int Userid { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}