using MediatRMessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace Carcass.Application.MasterData.UpdateOneRecord;

public sealed class MdUpdateOneRecordRequestCommand : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdUpdateOneRecordRequestCommand(string tableName, HttpRequest httpRequest, int id)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
        Id = id;
    }

    public HttpRequest HttpRequest { get; set; } //+

    public string TableName { get; set; }
    public int Id { get; set; }
}