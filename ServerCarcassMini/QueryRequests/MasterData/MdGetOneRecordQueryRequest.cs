using CarcassMasterDataDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.QueryRequests.MasterData;

public sealed class MdGetOneRecordQueryRequest : IQuery<MasterDataCrudLoadedData>
{
    public MdGetOneRecordQueryRequest(string tableName, int id, HttpRequest httpRequest)
    {
        TableName = tableName;
        Id = id;
        HttpRequest = httpRequest;
    }

    public string TableName { get; set; }
    public int Id { get; set; }
    public HttpRequest HttpRequest { get; set; }
}