using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.Registration;

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

    public async Task<Result<LoginResponse>> Handle(RegistrationRequestCommand request,
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
        Result<LoginResult> tryLoginResult =
            await _registrationService.TryToRegister(registerParameters, cancellationToken);
        if (tryLoginResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(tryLoginResult.Error);
        }

        AppUser user = tryLoginResult.Value.User;
        var appUserModel = new LoginResponse(user.Id, user.UserName!, user.Email!, tryLoginResult.Value.Token,
            user.FirstName, user.LastName, string.Empty);
        return appUserModel;
    }
}
