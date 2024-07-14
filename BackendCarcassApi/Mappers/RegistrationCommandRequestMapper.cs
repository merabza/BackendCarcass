using BackendCarcassApi.CommandRequests.Authentication;
using BackendCarcassContracts.V1.Requests;

namespace BackendCarcassApi.Mappers;

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