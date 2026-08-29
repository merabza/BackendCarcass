using System.Collections.Generic;
using BackendCarcass.Application.Crud.Models;

namespace BackendCarcass.Application.FilterSort.Models;

public record FilterSortObject(
    int TabWindowId,
    string TableName,
    List<ColumnFilter> FilterByFields,
    List<SortField> SortByFields);
