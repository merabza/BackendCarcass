using BackendCarcassApi.QueryResponses;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed class MdGetLookupTablesQueryRequest : IQuery<MdGetLookupTablesQueryResponse>
{
    public MdGetLookupTablesQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }
}