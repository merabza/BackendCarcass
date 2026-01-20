using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.DataTypes.GetMultipleGridModels;

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
