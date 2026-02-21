using BackendCarcass.Application.Authentication.Registration;
using BackendCarcassShared.BackendCarcassContracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class RegistrationCommandRequestMapper
{
    public static RegistrationRequestCommand AdaptTo(this RegistrationRequest registrationRequest)
    {
        return new RegistrationRequestCommand
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
