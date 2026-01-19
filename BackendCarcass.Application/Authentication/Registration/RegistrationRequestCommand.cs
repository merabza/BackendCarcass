using BackendCarcassContracts.V1.Responses;
using MediatRMessagingAbstractions;

namespace Carcass.Application.Authentication.Registration;

public sealed class RegistrationRequestCommand : ICommand<LoginResponse>
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}