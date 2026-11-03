using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.ConfirmEmail;

public sealed class ConfirmEmailRequestCommand : ICommand<LoginResponse>
{
    public string? PendingToken { get; set; }
    public string? Code { get; set; }
}
