using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassContracts.V1.Requests;

namespace BackendCarcassApi.Mappers;

public static class ChangeProfileCommandRequestMapper
{
    public static ChangeProfileCommandRequest AdaptTo(this ChangeProfileRequest changeProfileRequest)
    {
        return new ChangeProfileCommandRequest
        {
            Userid = changeProfileRequest.Userid,
            UserName = changeProfileRequest.UserName,
            Email = changeProfileRequest.Email,
            FirstName = changeProfileRequest.FirstName,
            LastName = changeProfileRequest.LastName
        };
    }
}