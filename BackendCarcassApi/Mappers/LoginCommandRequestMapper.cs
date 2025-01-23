using BackendCarcassApi.CommandRequests.Authentication;
using BackendCarcassContracts.V1.Requests;

namespace BackendCarcassApi.Mappers;

public static class LoginCommandRequestMapper
{
    public static LoginCommandRequest AdaptTo(this LoginRequest loginRequest)
    {
        return new LoginCommandRequest { UserName = loginRequest.UserName, Password = loginRequest.Password };
    }
}