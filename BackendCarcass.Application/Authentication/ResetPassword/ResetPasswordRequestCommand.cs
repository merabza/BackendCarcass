using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.ResetPassword;

public sealed class ResetPasswordRequestCommand : ICommand<LoginResponse>
{
    public int? UserId { get; set; }
    public string? Token { get; set; }
    public string? UserNameOrEmail { get; set; }
    public string? Code { get; set; }
    public string? NewPassword { get; set; }
    public string? NewPasswordConfirm { get; set; }
}
