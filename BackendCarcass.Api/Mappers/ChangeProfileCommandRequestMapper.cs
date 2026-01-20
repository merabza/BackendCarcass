using BackendCarcass.Application.UserRights.ChangeProfile;
using BackendCarcassContracts.V1.Requests;

namespace BackendCarcass.Api.Mappers;

public static class ChangeProfileCommandRequestMapper
{
    public static ChangeProfileRequestCommand AdaptTo(this ChangeProfileRequest changeProfileRequest)
    {
        return new ChangeProfileRequestCommand
        {
            Userid = changeProfileRequest.Userid,
            UserName = changeProfileRequest.UserName,
            Email = changeProfileRequest.Email,
            FirstName = changeProfileRequest.FirstName,
            LastName = changeProfileRequest.LastName
        };
    }
}