using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.CommandRequests.MasterData;

public sealed class MdUpdateOneRecordCommandRequest : ICommand
{
    public MdUpdateOneRecordCommandRequest(string tableName, HttpRequest httpRequest, int id)
    {
        TableName = tableName;
        HttpRequest = httpRequest;
        Id = id;
    }

    public HttpRequest HttpRequest { get; set; }

    public string TableName { get; set; }
    public int Id { get; set; }
}