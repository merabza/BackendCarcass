using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.ForgotPassword;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ForgotPasswordCommandHandler(PasswordResetService passwordResetService)
    : ICommandHandler<ForgotPasswordRequestCommand>
{
    public Task<Result> Handle(ForgotPasswordRequestCommand request, CancellationToken cancellationToken)
    {
        return passwordResetService.ForgotPasswordAsync(request.UserNameOrEmail, cancellationToken);
    }
}
