using BackendCarcass.Application.Authentication.ResetPassword;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ResetPasswordCommandRequestMapper
{
    public static ResetPasswordRequestCommand AdaptTo(this ResetPasswordRequest request)
    {
        return new ResetPasswordRequestCommand
        {
            UserId = request.UserId,
            Token = request.Token,
            UserNameOrEmail = request.UserNameOrEmail,
            Code = request.Code,
            NewPassword = request.NewPassword,
            NewPasswordConfirm = request.NewPasswordConfirm
        };
    }
}
