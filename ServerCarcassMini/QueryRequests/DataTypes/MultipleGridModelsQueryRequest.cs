using System.Collections.Generic;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.QueryRequests.DataTypes;

public sealed class MultipleGridModelsQueryRequest : IQuery<Dictionary<string, string>>
{
    public MultipleGridModelsQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }
}