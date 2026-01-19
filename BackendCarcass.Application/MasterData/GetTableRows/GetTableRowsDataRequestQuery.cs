using LibCrud.Models;
using MediatRMessagingAbstractions;

namespace Carcass.Application.MasterData.GetTableRows;

public record GetTableRowsDataRequestQuery(string TableName, string FilterSortRequest) : IQuery<TableRowsData>;