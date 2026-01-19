using CarcassMasterData.Models;
using MediatRMessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace Carcass.Application.MasterData.CreateOneRecord;

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