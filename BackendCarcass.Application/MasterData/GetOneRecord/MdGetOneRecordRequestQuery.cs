using BackendCarcass.MasterData.Models;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.MasterData.GetOneRecord;

public sealed class MdGetOneRecordRequestQuery : IQuery<MasterDataCrudLoadedData>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetOneRecordRequestQuery(string tableName, int id)
    {
        TableName = tableName;
        Id = id;
    }

    public string TableName { get; set; }
    public int Id { get; set; }
}