using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassContracts.V1.Requests;

namespace BackendCarcassApi.Mappers;

public static class ChangePasswordCommandRequestMapper
{
    public static ChangePasswordCommandRequest AdaptTo(this ChangePasswordRequest changePasswordRequest)
    {
        return new ChangePasswordCommandRequest(changePasswordRequest.Userid, changePasswordRequest.UserName,
            changePasswordRequest.OldPassword, changePasswordRequest.NewPassword,
            changePasswordRequest.NewPasswordConfirm);
    }
}