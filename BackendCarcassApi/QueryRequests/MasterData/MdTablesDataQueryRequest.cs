using BackendCarcassApi.QueryResponses;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed class MdTablesDataQueryRequest : IQuery<MdTablesDataQueryResponse>
{
    public MdTablesDataQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }
}