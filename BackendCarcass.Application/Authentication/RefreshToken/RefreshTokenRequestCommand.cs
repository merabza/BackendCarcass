using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.RefreshToken;

public sealed class RefreshTokenRequestCommand : ICommand<LoginResponse>
{
    public string? RefreshToken { get; set; }
}
