using BackendCarcassApi.QueryResponses;
using MediatRMessagingAbstractions;
using Microsoft.Extensions.Primitives;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed class MdGetLookupTablesQueryRequest : IQuery<MdGetLookupTablesQueryResponse>
{
    //StringValues tables
    //public MdGetLookupTablesQueryRequest(HttpRequest httpRequest)
    //{
    //    HttpRequest = httpRequest;
    //}

    //public HttpRequest HttpRequest { get; init; } //+

    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetLookupTablesQueryRequest(StringValues tables)
    {
        Tables = tables;
    }

    public StringValues Tables { get; init; } //+
}