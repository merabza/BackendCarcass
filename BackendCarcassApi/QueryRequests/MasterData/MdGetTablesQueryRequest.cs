using BackendCarcassApi.QueryResponses;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed class MdGetTablesQueryRequest : IQuery<MdGetTablesQueryResponse>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetTablesQueryRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }//+
}