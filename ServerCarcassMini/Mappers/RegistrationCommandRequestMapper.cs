using CarcassContracts.V1.Requests;
using ServerCarcassMini.CommandRequests.Authentication;

namespace ServerCarcassMini.Mappers;

public static class RegistrationCommandRequestMapper
{
    public static RegistrationCommandRequest AdaptTo(this RegistrationRequest registrationRequest)
    {
        return new RegistrationCommandRequest
        {
            Email = registrationRequest.Email,
            FirstName = registrationRequest.FirstName,
            LastName = registrationRequest.LastName,
            UserName = registrationRequest.UserName,
            Password = registrationRequest.Password,
            ConfirmPassword = registrationRequest.ConfirmPassword
        };
    }
}