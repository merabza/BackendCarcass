using BackendCarcassApi.QueryResponses;
using MediatRMessagingAbstractions;
using Microsoft.Extensions.Primitives;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed class MdGetTablesQueryRequest : IQuery<MdGetTablesQueryResponse>
{
    //public MdGetTablesQueryRequest(HttpRequest httpRequest)
    //{
    //    HttpRequest = httpRequest;
    //}

    //public HttpRequest HttpRequest { get; set; } //+

    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetTablesQueryRequest(StringValues tables)
    {
        Tables = tables;
    }

    public StringValues Tables { get; init; } //+
}