using LibCrud.Models;
using MessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.MasterData;

public record GetTableRowsDataQueryRequest(string TableName, string FilterSortRequest) : IQuery<TableRowsData>;