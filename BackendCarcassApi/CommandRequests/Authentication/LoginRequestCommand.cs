using BackendCarcassContracts.V1.Responses;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.Authentication;

public sealed class LoginRequestCommand : ICommand<LoginResponse>
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}