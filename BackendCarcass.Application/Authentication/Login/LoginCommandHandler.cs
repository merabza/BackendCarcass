using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.Login;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class LoginCommandHandler(LoginService loginService)
    : LoginCommandHandlerBase, ICommandHandler<LoginRequestCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginRequestCommand request, CancellationToken cancellationToken)
    {
        Result<LoginResult> tryLoginResult =
            await loginService.TryToLogin(request.UserName!, request.Password!, cancellationToken);
        if (tryLoginResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(tryLoginResult.Error);
        }

        AppUser user = tryLoginResult.Value.User;
        var appUserModel = new LoginResponse(user.Id, user.UserName!, user.Email!, tryLoginResult.Value.Token,
            tryLoginResult.Value.Roles.Aggregate(string.Empty,
                (cur, next) => cur + (string.IsNullOrEmpty(cur) ? string.Empty : ", ") + next), user.FirstName,
            user.LastName, tryLoginResult.Value.AppClaims);
        return Result.Success(appUserModel);
    }
}
