using LibCrud.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.MasterData;

public record GetTableRowsDataRequestQuery(string TableName, string FilterSortRequest) : IQuery<TableRowsData>;