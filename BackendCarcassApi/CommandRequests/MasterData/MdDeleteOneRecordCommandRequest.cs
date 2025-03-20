using MessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.MasterData;

public sealed class MdDeleteOneRecordCommandRequest : ICommand
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdDeleteOneRecordCommandRequest(string tableName, int id)
    {
        TableName = tableName;
        Id = id;
    }

    public string TableName { get; set; }
    public int Id { get; set; }
}