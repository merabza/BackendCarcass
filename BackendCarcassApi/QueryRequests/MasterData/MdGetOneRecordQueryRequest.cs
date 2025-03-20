using CarcassMasterDataDom.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed class MdGetOneRecordQueryRequest : IQuery<MasterDataCrudLoadedData>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetOneRecordQueryRequest(string tableName, int id)
    {
        TableName = tableName;
        Id = id;
    }

    public string TableName { get; set; }
    public int Id { get; set; }
}