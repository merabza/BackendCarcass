using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.Authentication.Login;

public sealed class LoginRequestCommand : ICommandOmd<LoginResponse>
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
