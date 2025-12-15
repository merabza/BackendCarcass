namespace CarcassApplication.Services.Authentication.Models;

public class RegisterParameters
{
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

}