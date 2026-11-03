using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
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
        return tryLoginResult.IsFailure
            ? Result.Failure<LoginResponse>(tryLoginResult.Error)
            : tryLoginResult.Value.ToLoginResponse();
    }
}
