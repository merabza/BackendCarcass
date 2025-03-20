using LibCrud.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.MasterData;

public record GetTableRowsDataQueryRequest(string TableName, string FilterSortRequest) : IQuery<TableRowsData>;