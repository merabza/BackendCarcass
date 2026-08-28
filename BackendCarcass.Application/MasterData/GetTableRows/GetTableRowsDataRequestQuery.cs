using BackendCarcass.LibCrud.Models;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.MasterData.GetTableRows;

public record GetTableRowsDataRequestQuery(string TableName, string FilterSortRequest) : IQuery<TableRowsData>;
