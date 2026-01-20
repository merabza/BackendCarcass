using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcassContracts.V1.Responses;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Authentication.Login;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LoginCommandHandler : LoginCommandHandlerBase, ICommandHandler<LoginRequestCommand, LoginResponse>
{
    private readonly LoginService _loginService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public LoginCommandHandler(LoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<OneOf<LoginResponse, Err[]>> Handle(LoginRequestCommand request,
        CancellationToken cancellationToken)
    {
        OneOf<LoginResult, Err[]> tryLoginResult =
            await _loginService.TryToLogin(request.UserName!, request.Password!, cancellationToken);
        if (tryLoginResult.IsT1)
        {
            return tryLoginResult.AsT1;
        }

        AppUser user = tryLoginResult.AsT0.User;
        var appUserModel = new LoginResponse(user.Id, user.UserName!, user.Email!, tryLoginResult.AsT0.Token,
            tryLoginResult.AsT0.Roles.Aggregate(string.Empty,
                (cur, next) => cur + (string.IsNullOrEmpty(cur) ? string.Empty : ", ") + next), user.FirstName,
            user.LastName, tryLoginResult.AsT0.AppClaims);
        return appUserModel;
    }
}
