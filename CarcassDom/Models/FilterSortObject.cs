using System.Collections.Generic;
using LibCrud.Models;

namespace CarcassDom.Models;

public record FilterSortObject(int TabWindowId, string TableName, List<ColumnFilter> FilterByFields,
    List<SortField> SortByFields);
