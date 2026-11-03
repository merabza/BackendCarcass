using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Services.Authentication;
using BackendCarcass.Application.Services.Authentication.Models;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Authentication.ResetPassword;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ResetPasswordCommandHandler(PasswordResetService passwordResetService)
    : ICommandHandler<ResetPasswordRequestCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(ResetPasswordRequestCommand request,
        CancellationToken cancellationToken)
    {
        var parameters = new ResetPasswordParameters
        {
            UserId = request.UserId,
            Token = request.Token,
            UserNameOrEmail = request.UserNameOrEmail,
            Code = request.Code,
            NewPassword = request.NewPassword!
        };
        Result<LoginResult> result = await passwordResetService.ResetPasswordAsync(parameters, cancellationToken);
        return result.IsFailure ? Result.Failure<LoginResponse>(result.Error) : result.Value.ToLoginResponse();
    }
}
