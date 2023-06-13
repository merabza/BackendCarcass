using CarcassContracts.V1.Responses;
using MessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.Authentication;

public sealed class LoginCommandRequest : ICommand<LoginResponse>
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}