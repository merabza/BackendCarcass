using BackendCarcass.Application.Authentication.ConfirmEmail;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ConfirmEmailCommandRequestMapper
{
    public static ConfirmEmailRequestCommand AdaptTo(this ConfirmEmailRequest request)
    {
        return new ConfirmEmailRequestCommand { PendingToken = request.PendingToken, Code = request.Code };
    }
}
