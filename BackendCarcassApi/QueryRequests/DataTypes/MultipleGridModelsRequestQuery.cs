using System.Collections.Generic;
using MediatRMessagingAbstractions;
using Microsoft.Extensions.Primitives;

namespace BackendCarcassApi.QueryRequests.DataTypes;

public sealed class MultipleGridModelsRequestQuery : IQuery<Dictionary<string, string>>
{
    //public MultipleGridModelsQueryRequest(HttpRequest httpRequest)
    //{
    //    HttpRequest = httpRequest;
    //}

    //public HttpRequest HttpRequest { get; set; } //+

    // ReSharper disable once ConvertToPrimaryConstructor
    public MultipleGridModelsRequestQuery(StringValues grids)
    {
        Grids = grids;
    }

    public StringValues Grids { get; init; } //+
}