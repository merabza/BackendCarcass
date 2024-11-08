using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class DeleteCurrentUserCommandRequest : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteCurrentUserCommandRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public string? UserName { get; set; }
    public HttpRequest HttpRequest { get; set; }
}