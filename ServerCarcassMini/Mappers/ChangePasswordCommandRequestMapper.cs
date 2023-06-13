﻿using CarcassContracts.V1.Requests;
using Microsoft.AspNetCore.Http;
using ServerCarcassMini.CommandRequests.UserRights;

namespace ServerCarcassMini.Mappers;

public static class ChangePasswordCommandRequestMapper
{
    public static ChangePasswordCommandRequest AdaptTo(this ChangePasswordRequest changePasswordRequest,
        HttpRequest httpRequest)
    {
        return new ChangePasswordCommandRequest(changePasswordRequest.Userid, changePasswordRequest.UserName,
            changePasswordRequest.OldPassword, changePasswordRequest.NewPassword,
            changePasswordRequest.NewPasswordConfirm, httpRequest);
    }
}