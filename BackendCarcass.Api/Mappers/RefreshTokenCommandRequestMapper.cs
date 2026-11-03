using BackendCarcass.Application.Authentication.RefreshToken;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class RefreshTokenCommandRequestMapper
{
    public static RefreshTokenRequestCommand AdaptTo(this RefreshTokenRequest request)
    {
        return new RefreshTokenRequestCommand { RefreshToken = request.RefreshToken };
    }
}
