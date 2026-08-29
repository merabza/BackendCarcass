using System.Collections.Generic;
using BackendCarcass.Application.MasterData.Models;

//using AppUser = BackendCarcass.Application.MasterData.Models.AppUser;

namespace BackendCarcass.Application.Services.Authentication.Models;

public class LoginResult
{
    public required AppUser User { get; set; }
    public int LastSequentialNumber { get; set; }
    public required string Token { get; set; }
    public required List<string> AppClaims { get; set; }
    public required IList<string> Roles { get; set; }
}
