using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.ResendCode;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ResendCodeCommandHandler(PendingLoginService pendingLoginService)
    : ICommandHandler<ResendCodeRequestCommand>
{
    public Task<Result> Handle(ResendCodeRequestCommand request, CancellationToken cancellationToken)
    {
        return pendingLoginService.ResendCodeAsync(request.PendingToken, cancellationToken);
    }
}
