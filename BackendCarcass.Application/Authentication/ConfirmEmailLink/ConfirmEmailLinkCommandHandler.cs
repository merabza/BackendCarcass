using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.ConfirmEmailLink;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ConfirmEmailLinkCommandHandler(PendingLoginService pendingLoginService)
    : ICommandHandler<ConfirmEmailLinkRequestCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(ConfirmEmailLinkRequestCommand request,
        CancellationToken cancellationToken)
    {
        Result<LoginResult> result = await pendingLoginService.ConfirmEmailByLinkAsync(request.UserId,
            request.Token, request.PendingToken, cancellationToken);
        return result.IsFailure ? Result.Failure<LoginResponse>(result.Error) : result.Value.ToLoginResponse();
    }
}
