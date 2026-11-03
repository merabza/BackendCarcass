using System.Collections.Generic;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcassShared.Contracts.V1.Responses;

//using AppUser = BackendCarcass.Application.MasterData.Models.AppUser;

namespace BackendCarcass.Application.Services.Authentication.Models;

public class LoginResult
{
    public required AppUser User { get; set; }
    public int LastSequentialNumber { get; set; }

    //სრული წვდომის ტოკენი, თუ NextStep არის None. სხვა შემთხვევაში დროებითი ტოკენი მომდევნო ნაბიჯისთვის
    public required string Token { get; set; }

    //ივსება მხოლოდ სრული წვდომის შემთხვევაში
    public string? RefreshToken { get; set; }

    public ELoginNextStep NextStep { get; set; }
    public required List<string> AppClaims { get; set; }
    public required IList<string> Roles { get; set; }
}
