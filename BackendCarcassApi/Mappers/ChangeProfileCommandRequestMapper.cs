using BackendCarcassContracts.V1.Requests;
using Carcass.Application.UserRights.ChangeProfile;

namespace BackendCarcassApi.Mappers;

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