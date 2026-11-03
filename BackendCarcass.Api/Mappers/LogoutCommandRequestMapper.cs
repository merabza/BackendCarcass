using BackendCarcass.Application.Authentication.Logout;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class LogoutCommandRequestMapper
{
    public static LogoutRequestCommand AdaptTo(this LogoutRequest request)
    {
        return new LogoutRequestCommand { RefreshToken = request.RefreshToken };
    }
}
