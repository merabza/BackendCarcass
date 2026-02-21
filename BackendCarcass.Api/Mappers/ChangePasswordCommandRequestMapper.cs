using BackendCarcass.Application.UserRights.ChangePassword;
using BackendCarcassShared.BackendCarcassContracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ChangePasswordCommandRequestMapper
{
    public static ChangePasswordRequestCommand AdaptTo(this ChangePasswordRequest changePasswordRequest)
    {
        return new ChangePasswordRequestCommand(changePasswordRequest.Userid, changePasswordRequest.UserName,
            changePasswordRequest.OldPassword, changePasswordRequest.NewPassword,
            changePasswordRequest.NewPasswordConfirm);
    }
}
