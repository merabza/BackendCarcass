using CarcassContracts.V1.Requests;
using Microsoft.AspNetCore.Http;
using ServerCarcassMini.CommandRequests.UserRights;

namespace ServerCarcassMini.Mappers;

public static class ChangeProfileCommandRequestMapper
{
    public static ChangeProfileCommandRequest AdaptTo(this ChangeProfileRequest changeProfileRequest,
        HttpRequest httpRequest)
    {
        return new ChangeProfileCommandRequest(httpRequest)
        {
            Userid = changeProfileRequest.Userid,
            UserName = changeProfileRequest.UserName,
            Email = changeProfileRequest.Email,
            FirstName = changeProfileRequest.FirstName,
            LastName = changeProfileRequest.LastName
        };
    }
}