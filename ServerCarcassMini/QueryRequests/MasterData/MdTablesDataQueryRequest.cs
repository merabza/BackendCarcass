using MessagingAbstractions;
using Microsoft.AspNetCore.Http;
using ServerCarcassMini.QueryResponses;

namespace ServerCarcassMini.QueryRequests.MasterData;

public sealed class MdTablesDataQueryRequest : IQuery<MdTablesDataQueryResponse>
{
    public MdTablesDataQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }
}