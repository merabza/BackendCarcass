using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BackendCarcass.MasterData.Models;

public sealed class AppUser : IdentityUser<int>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AppUser(string userName, string firstName, string lastName) : base(userName)
    {
        FirstName = firstName;
        LastName = lastName;
        FullName = firstName + " " + lastName;
    }

    // no additional members are required
    // for basic Identity installation
    //public string Token { get; set; }
    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string? CreateJwToken(string secret, int serialNumber, IList<string>? roles = null)
    {
        // authentication successful so generate jwt token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email)) return null;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, UserName),
            new(ClaimTypes.Email, Email)
        };
        if (roles is not null) claims.AddRange(roles.Select(s => new Claim(ClaimTypes.Role, s)));

        claims.Add(new Claim(ClaimTypes.SerialNumber, serialNumber.ToString(CultureInfo.InvariantCulture)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims.ToArray()),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}