using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Authentication;
using BackendCarcassContracts.V1.Responses;
using CarcassApplication.Services.Authentication;
using CarcassApplication.Services.Authentication.Models;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.Authentication;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RegistrationCommandHandler : LoginCommandBase,
    ICommandHandler<RegistrationRequestCommand, LoginResponse>
{
    private readonly RegistrationService _registrationService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RegistrationCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<OneOf<LoginResponse, Err[]>> Handle(RegistrationRequestCommand request,
        CancellationToken cancellationToken = default)
    {
        var registerParameters = new RegisterParameters
        {
            UserName = request.UserName!,
            Email = request.Email!,
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            Password = request.Password!
        };
        var tryLoginResult = await _registrationService.TryToRegister(registerParameters, cancellationToken);
        if (tryLoginResult.IsT1)
            return tryLoginResult.AsT1;

        var user = tryLoginResult.AsT0.User;
        var appUserModel = new LoginResponse(user.Id, LastSequentialNumber, user.UserName!, user.Email!,
            tryLoginResult.AsT0.Token, user.FirstName, user.LastName, string.Empty);
        return appUserModel;
    }
}