using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class ChangeProfileCommandRequest : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangeProfileCommandRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public int Userid { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public HttpRequest HttpRequest { get; set; }
}