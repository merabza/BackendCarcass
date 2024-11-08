using CarcassRepositories.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.UserRights;

public sealed class MainMenuQueryRequest : IQuery<MainMenuModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MainMenuQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }
}