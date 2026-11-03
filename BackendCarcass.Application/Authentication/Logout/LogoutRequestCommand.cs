using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.Logout;

public sealed class LogoutRequestCommand : ICommand
{
    public string? RefreshToken { get; set; }
}
