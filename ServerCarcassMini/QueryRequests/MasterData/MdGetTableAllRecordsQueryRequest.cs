using MessagingAbstractions;
using Microsoft.AspNetCore.Http;
using ServerCarcassMini.QueryResponses;

namespace ServerCarcassMini.QueryRequests.MasterData;

public sealed class MdGetTableAllRecordsQueryRequest : IQuery<MdGetTableAllRecordsQueryResponse>
{
    public MdGetTableAllRecordsQueryRequest(string tableName, HttpRequest httpRequest)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
    }

    public string TableName { get; set; }
    public HttpRequest HttpRequest { get; set; }
}