namespace CarcassDom.Models;

public record FilterSortObject(int TabWindowId, string TableName, List<SortField> FilterByFields,
    List<SortField> SortByFields);