namespace BackendCarcass.Application.Services.Authentication.Models;

//პაროლის აღდგენა ან ბმულით (UserId + Token), ან კოდით (UserNameOrEmail + Code)
public sealed class ResetPasswordParameters
{
    public int? UserId { get; init; }
    public string? Token { get; init; }
    public string? UserNameOrEmail { get; init; }
    public string? Code { get; init; }
    public required string NewPassword { get; init; }
}
