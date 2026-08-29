using System.Collections.Generic;
using BackendCarcass.Application.FilterSort.Models;
using BackendCarcass.Application.Identity;

namespace BackendCarcass.Application.FilterSort;

public sealed class FilterSortManager
{
    private readonly Dictionary<FilterSortIdentifier, FilterSortObject> _filterSortObjects = new();

    public void Use(ICurrentUser currentUser, FilterSortObject filterSortObject)
    {
        var filterSortIdentifier = new FilterSortIdentifier(currentUser.SerialNumber, filterSortObject.TabWindowId,
            filterSortObject.TableName);
        _filterSortObjects[filterSortIdentifier] = filterSortObject;
    }

    public FilterSortObject? Get(ICurrentUser currentUser, int tabWindowId, string tableName)
    {
        var filterSortIdentifier = new FilterSortIdentifier(currentUser.SerialNumber, tabWindowId, tableName);
        return _filterSortObjects.GetValueOrDefault(filterSortIdentifier);
    }
}
