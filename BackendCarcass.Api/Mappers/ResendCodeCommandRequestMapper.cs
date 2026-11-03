using BackendCarcass.Application.Authentication.ResendCode;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ResendCodeCommandRequestMapper
{
    public static ResendCodeRequestCommand AdaptTo(this ResendCodeRequest request)
    {
        return new ResendCodeRequestCommand { PendingToken = request.PendingToken };
    }
}
