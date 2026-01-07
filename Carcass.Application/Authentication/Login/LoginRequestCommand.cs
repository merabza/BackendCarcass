using BackendCarcassContracts.V1.Responses;
using MediatRMessagingAbstractions;

namespace Carcass.Application.Authentication.Login;

public sealed class LoginRequestCommand : ICommand<LoginResponse>
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}