using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.CommandRequests.UserRights;

public sealed class DeleteCurrentUserCommandRequest : ICommand
{
    public DeleteCurrentUserCommandRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public string? UserName { get; set; }
    public HttpRequest HttpRequest { get; set; }
}