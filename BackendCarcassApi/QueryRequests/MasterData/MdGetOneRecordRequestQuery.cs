using CarcassMasterDataDom.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.MasterData;

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