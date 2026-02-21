using BackendCarcass.Application.Authentication.Login;
using BackendCarcassShared.BackendCarcassContracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class LoginCommandRequestMapper
{
    public static LoginRequestCommand AdaptTo(this LoginRequest loginRequest)
    {
        return new LoginRequestCommand { UserName = loginRequest.UserName, Password = loginRequest.Password };
    }
}
