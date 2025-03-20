using System.Collections.Generic;
using MediatRMessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.DataTypes;

public sealed class MultipleGridModelsQueryRequest : IQuery<Dictionary<string, string>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MultipleGridModelsQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; } //+
}