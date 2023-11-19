namespace LibCrud.Models;

public record FilterSortRequest(int Offset, int RowsCount, ColumnFilter[]? FilterFields, SortField[]? SortByFields);