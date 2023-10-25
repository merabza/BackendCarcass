using CarcassMasterDataDom.Models;
using MessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.MasterData;

public record GetTableRowsDataQueryRequest(string tableName, string FilterSortRequest) : IQuery<TableRowsData>;