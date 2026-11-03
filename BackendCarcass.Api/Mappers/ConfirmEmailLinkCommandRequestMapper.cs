using BackendCarcass.Application.Authentication.ConfirmEmailLink;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ConfirmEmailLinkCommandRequestMapper
{
    public static ConfirmEmailLinkRequestCommand AdaptTo(this ConfirmEmailLinkRequest request)
    {
        return new ConfirmEmailLinkRequestCommand
        {
            UserId = request.UserId, Token = request.Token, PendingToken = request.PendingToken
        };
    }
}
