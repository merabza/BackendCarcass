using System.Collections.Generic;

namespace CarcassContracts.V1.Responses;

public sealed class LoginResponse
{
    public LoginResponse(int userId, int sequentialNumber, string userName, string email, string token,
        string firstName,
        string lastName, string roleName)
    {
        UserId = userId;
        SequentialNumber = sequentialNumber;
        UserName = userName;
        Email = email;
        Token = token;
        FirstName = firstName;
        LastName = lastName;
        RoleName = roleName;
    }

    public LoginResponse(int userId, int sequentialNumber, string userName, string email, string token, string roleName,
        string firstName, string lastName, List<string> appClaims)
    {
        UserId = userId;
        SequentialNumber = sequentialNumber;
        UserName = userName;
        Email = email;
        Token = token;
        RoleName = roleName;
        FirstName = firstName;
        LastName = lastName;
        AppClaims = appClaims;
    }

    public string UserName { get; set; }
    public string Email { get; set; }
    public int UserId { get; set; }
    public int SequentialNumber { get; set; }
    public string Token { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string RoleName { get; set; }
    public List<string> AppClaims { get; set; } = new();
}