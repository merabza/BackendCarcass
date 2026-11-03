using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.ChangeRequiredPassword;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChangeRequiredPasswordCommandHandler(PendingLoginService pendingLoginService)
    : ICommandHandler<ChangeRequiredPasswordRequestCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(ChangeRequiredPasswordRequestCommand request,
        CancellationToken cancellationToken)
    {
        Result<LoginResult> result = await pendingLoginService.ChangeRequiredPasswordAsync(request.PendingToken,
            request.Code, request.NewPassword, cancellationToken);
        return result.IsFailure ? Result.Failure<LoginResponse>(result.Error) : result.Value.ToLoginResponse();
    }
}
