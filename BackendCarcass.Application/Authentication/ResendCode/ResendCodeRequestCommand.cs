using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.ResendCode;

public sealed class ResendCodeRequestCommand : ICommand
{
    public string? PendingToken { get; set; }
}
