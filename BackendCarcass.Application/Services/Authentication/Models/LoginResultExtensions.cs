using BackendCarcass.Application.MasterData.Models;
using BackendCarcassShared.Contracts.V1.Responses;

namespace BackendCarcass.Application.Services.Authentication.Models;

public static class LoginResultExtensions
{
    public static LoginResponse ToLoginResponse(this LoginResult loginResult)
    {
        AppUser user = loginResult.User;
        return new LoginResponse(user.Id, user.UserName!, user.Email!, loginResult.Token,
            string.Join(", ", loginResult.Roles), user.FirstName, user.LastName, loginResult.AppClaims)
        {
            NextStep = loginResult.NextStep, RefreshToken = loginResult.RefreshToken
        };
    }
}
