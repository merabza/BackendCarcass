using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.ConfirmEmail;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ConfirmEmailCommandHandler(PendingLoginService pendingLoginService)
    : ICommandHandler<ConfirmEmailRequestCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(ConfirmEmailRequestCommand request,
        CancellationToken cancellationToken)
    {
        Result<LoginResult> result =
            await pendingLoginService.ConfirmEmailByCodeAsync(request.PendingToken, request.Code, cancellationToken);
        return result.IsFailure ? Result.Failure<LoginResponse>(result.Error) : result.Value.ToLoginResponse();
    }
}
