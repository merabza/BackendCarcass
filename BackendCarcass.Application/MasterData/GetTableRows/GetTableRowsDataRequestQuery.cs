using BackendCarcass.LibCrud.Models;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.MasterData.GetTableRows;

public record GetTableRowsDataRequestQuery(string TableName, string FilterSortRequest) : IQuery<TableRowsData>;