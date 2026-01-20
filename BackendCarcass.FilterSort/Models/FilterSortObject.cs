using System.Collections.Generic;
using BackendCarcass.LibCrud.Models;

namespace BackendCarcass.FilterSort.Models;

public record FilterSortObject(
    int TabWindowId,
    string TableName,
    List<ColumnFilter> FilterByFields,
    List<SortField> SortByFields);
