using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Http;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.MasterData.CreateOneRecord;

public sealed class MdCreateOneRecordRequestCommand : ICommand<MasterDataCrudLoadedData>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdCreateOneRecordRequestCommand(string tableName, HttpRequest httpRequest)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; } //++

    public string TableName { get; set; }
}