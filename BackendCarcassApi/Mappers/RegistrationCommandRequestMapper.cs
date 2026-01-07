using BackendCarcassContracts.V1.Requests;
using Carcass.Application.Authentication.Registration;

namespace BackendCarcassApi.Mappers;

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