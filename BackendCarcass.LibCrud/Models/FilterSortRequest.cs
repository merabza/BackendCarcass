namespace BackendCarcass.LibCrud.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public record FilterSortRequest(int Offset, int RowsCount, ColumnFilter[]? FilterFields, SortField[]? SortByFields);
