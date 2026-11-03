using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.Authentication.ConfirmEmailLink;

public sealed class ConfirmEmailLinkRequestCommand : ICommand<LoginResponse>
{
    public int UserId { get; set; }
    public string? Token { get; set; }
    public string? PendingToken { get; set; }
}
