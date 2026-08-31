using BackendCarcass.Application.MasterData.Models;
using Microsoft.AspNetCore.Http;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.MasterData.CreateOneRecord;

public sealed class MdCreateOneRecordRequestCommand : ICommand<MasterDataCrudLoadedData>
{
    public MdCreateOneRecordRequestCommand(string tableName, HttpRequest httpRequest)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; } //++

    public string TableName { get; set; }
}
