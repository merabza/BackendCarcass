using BackendCarcassApi.CommandRequests.Authentication;
using BackendCarcassContracts.V1.Requests;

namespace BackendCarcassApi.Mappers;

public static class LoginCommandRequestMapper
{
    public static LoginRequestCommand AdaptTo(this LoginRequest loginRequest)
    {
        return new LoginRequestCommand { UserName = loginRequest.UserName, Password = loginRequest.Password };
    }
}