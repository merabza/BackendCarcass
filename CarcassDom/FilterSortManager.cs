using System.Collections.Generic;
using CarcassDom.Models;
using CarcassIdentity;

namespace CarcassDom;

public class FilterSortManager
{
    private readonly Dictionary<FilterSortIdentifier, FilterSortObject> _filterSortObjects = new();

    public void Use(ICurrentUser currentUser, FilterSortObject filterSortObject)
    {
        FilterSortIdentifier filterSortIdentifier = new(currentUser.SerialNumber,
            filterSortObject.TabWindowId, filterSortObject.TableName);
        _filterSortObjects[filterSortIdentifier] = filterSortObject;
    }

    public FilterSortObject? Get(ICurrentUser currentUser, int tabWindowId, string tableName)
    {
        FilterSortIdentifier filterSortIdentifier = new(currentUser.SerialNumber, tabWindowId, tableName);
        return _filterSortObjects.GetValueOrDefault(filterSortIdentifier);
    }
}