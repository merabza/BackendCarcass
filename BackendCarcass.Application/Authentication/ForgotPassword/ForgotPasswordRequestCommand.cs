using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.ForgotPassword;

public sealed class ForgotPasswordRequestCommand : ICommand
{
    public string? UserNameOrEmail { get; set; }
}
