using BackendCarcassContracts.V1.Requests;
using Carcass.Application.UserRights.ChangePassword;

namespace BackendCarcassApi.Mappers;

public static class ChangePasswordCommandRequestMapper
{
    public static ChangePasswordRequestCommand AdaptTo(this ChangePasswordRequest changePasswordRequest)
    {
        return new ChangePasswordRequestCommand(changePasswordRequest.Userid, changePasswordRequest.UserName,
            changePasswordRequest.OldPassword, changePasswordRequest.NewPassword,
            changePasswordRequest.NewPasswordConfirm);
    }
}