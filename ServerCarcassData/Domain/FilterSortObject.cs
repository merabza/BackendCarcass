using System.Collections.Generic;
using ServerCarcassData.Contracts.V1.Requests;

namespace ServerCarcassData.Domain;

public sealed class FilterSortObject
{
    private FilterSortObject(int tabWindowId, string tableName, List<SortField> filterByFields,
        List<SortField> sortByFields)
    {
        TabWindowId = tabWindowId;
        TableName = tableName;
        FilterByFields = filterByFields;
        SortByFields = sortByFields;
    }

    public int TabWindowId { get; set; }
    public string TableName { get; set; }
    public List<SortField> FilterByFields { get; set; }
    public List<SortField> SortByFields { get; set; }


    public static FilterSortObject? Create(FilterSortObjectRequest filterSortObjectRequest)
    {
        if (filterSortObjectRequest.TabWindowId is null)
            return null;
        if (filterSortObjectRequest.TableName is null)
            return null;

        return new FilterSortObject(filterSortObjectRequest.TabWindowId.Value, filterSortObjectRequest.TableName,
            ConvertSortFields(filterSortObjectRequest.FilterByFields),
            ConvertSortFields(filterSortObjectRequest.SortByFields));
    }

    private static List<SortField> ConvertSortFields(List<SortFieldRequestPart> sfr)
    {
        List<SortField> sfl = new();
        foreach (var field in sfr)
            if (field.FieldName is not null)
                sfl.Add(new SortField(field.Ascending, field.FieldName));

        return sfl;
    }
}