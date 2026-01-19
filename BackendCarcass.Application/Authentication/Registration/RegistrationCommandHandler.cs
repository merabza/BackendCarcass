using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.V1.Responses;
using Carcass.Application.Services.Authentication;
using Carcass.Application.Services.Authentication.Models;
using CarcassMasterData.Models;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.Authentication.Registration;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class RegistrationCommandHandler : LoginCommandHandlerBase,
    ICommandHandler<RegistrationRequestCommand, LoginResponse>
{
    private readonly RegistrationService _registrationService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RegistrationCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public async Task<OneOf<LoginResponse, Err[]>> Handle(RegistrationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var registerParameters = new RegisterParameters
        {
            UserName = request.UserName!,
            Email = request.Email!,
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            Password = request.Password!
        };
        OneOf<LoginResult, Err[]> tryLoginResult =
            await _registrationService.TryToRegister(registerParameters, cancellationToken);
        if (tryLoginResult.IsT1)
        {
            return tryLoginResult.AsT1;
        }

        AppUser user = tryLoginResult.AsT0.User;
        var appUserModel = new LoginResponse(user.Id, user.UserName!, user.Email!, tryLoginResult.AsT0.Token,
            user.FirstName, user.LastName, string.Empty);
        return appUserModel;
    }
}
