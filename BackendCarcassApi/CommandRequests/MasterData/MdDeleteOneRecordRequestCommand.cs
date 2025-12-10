using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.MasterData;

public sealed class MdDeleteOneRecordRequestCommand : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdDeleteOneRecordRequestCommand(string tableName, int id)
    {
        TableName = tableName;
        Id = id;
    }

    public string TableName { get; set; }
    public int Id { get; set; }
}