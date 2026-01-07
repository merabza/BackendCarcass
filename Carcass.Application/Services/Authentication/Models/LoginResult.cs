using CarcassMasterDataDom.Models;

namespace Carcass.Application.Services.Authentication.Models;

public class LoginResult
{
    public required AppUser User { get; set; }
    public int LastSequentialNumber { get; set; }
    public required string Token { get; set; }
    public required List<string> AppClaims { get; set; }
    public required IList<string> Roles { get; set; }
}