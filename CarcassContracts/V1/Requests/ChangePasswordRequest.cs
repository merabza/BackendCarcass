namespace CarcassContracts.V1.Requests;

public sealed class ChangePasswordRequest
{
    public int Userid { get; set; }
    public string? UserName { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? NewPasswordConfirm { get; set; }
}