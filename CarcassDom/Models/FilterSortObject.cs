using System.Collections.Generic;

namespace CarcassDom.Models;

public record FilterSortObject(int TabWindowId, string TableName, List<ColumnFilter> FilterByFields,
    List<SortField> SortByFields);