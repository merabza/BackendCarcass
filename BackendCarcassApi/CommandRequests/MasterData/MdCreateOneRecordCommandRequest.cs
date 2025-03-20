using CarcassMasterDataDom.Models;
using MediatRMessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.MasterData;

public sealed class MdCreateOneRecordCommandRequest : ICommand<MasterDataCrudLoadedData>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdCreateOneRecordCommandRequest(string tableName, HttpRequest httpRequest)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
    }

    public HttpRequest HttpRequest { get; set; } //++

    public string TableName { get; set; }
}