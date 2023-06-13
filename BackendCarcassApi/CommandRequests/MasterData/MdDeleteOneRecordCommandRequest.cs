using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.MasterData;

public sealed class MdDeleteOneRecordCommandRequest : ICommand
{
    public MdDeleteOneRecordCommandRequest(string tableName, HttpRequest httpRequest, int id)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
        Id = id;
    }

    public HttpRequest HttpRequest { get; set; }

    public string TableName { get; set; }
    public int Id { get; set; }
}