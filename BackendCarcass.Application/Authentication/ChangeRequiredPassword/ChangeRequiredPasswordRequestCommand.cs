using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.ChangeRequiredPassword;

public sealed class ChangeRequiredPasswordRequestCommand : ICommand<LoginResponse>
{
    public string? PendingToken { get; set; }
    public string? Code { get; set; }
    public string? NewPassword { get; set; }
    public string? NewPasswordConfirm { get; set; }
}
