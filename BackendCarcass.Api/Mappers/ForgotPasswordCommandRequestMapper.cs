using BackendCarcass.Application.Authentication.ForgotPassword;
using BackendCarcassShared.Contracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ForgotPasswordCommandRequestMapper
{
    public static ForgotPasswordRequestCommand AdaptTo(this ForgotPasswordRequest request)
    {
        return new ForgotPasswordRequestCommand { UserNameOrEmail = request.UserNameOrEmail };
    }
}
