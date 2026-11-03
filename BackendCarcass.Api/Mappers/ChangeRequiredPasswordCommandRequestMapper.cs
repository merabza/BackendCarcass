using BackendCarcass.Application.Authentication.ChangeRequiredPassword;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ChangeRequiredPasswordCommandRequestMapper
{
    public static ChangeRequiredPasswordRequestCommand AdaptTo(this ChangeRequiredPasswordRequest request)
    {
        return new ChangeRequiredPasswordRequestCommand
        {
            PendingToken = request.PendingToken,
            Code = request.Code,
            NewPassword = request.NewPassword,
            NewPasswordConfirm = request.NewPasswordConfirm
        };
    }
}
