using CarcassContracts.V1.Requests;
using ServerCarcassMini.CommandRequests.Authentication;

namespace ServerCarcassMini.Mappers;

public static class LoginCommandRequestMapper
{
    public static LoginCommandRequest AdaptTo(this LoginRequest loginRequest)
    {
        return new LoginCommandRequest
        {
            UserName = loginRequest.UserName,
            Password = loginRequest.Password
        };
    }
}