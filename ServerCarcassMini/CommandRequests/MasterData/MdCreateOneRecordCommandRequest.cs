using CarcassMasterDataDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.CommandRequests.MasterData;

public sealed class MdCreateOneRecordCommandRequest : ICommand<MasterDataCrudLoadedData>
{
    public MdCreateOneRecordCommandRequest(string tableName, HttpRequest httpRequest)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; }

    public string TableName { get; set; }
}