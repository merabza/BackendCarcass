using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CarcassDom.Models;

namespace CarcassDom;

public class FilterSortManager
{
    private readonly Dictionary<FilterSortIdentifier, FilterSortObject> _filterSortObjects = new();

    private static int GetSerialNumber(IEnumerable<Claim> claims)
    {
        Claim? serialNumberClaim = claims.SingleOrDefault(so => so.Type == ClaimTypes.SerialNumber);
        if (serialNumberClaim == null)
            return 0;
        string strSerialNumber = serialNumberClaim.Value;
        return int.TryParse(strSerialNumber, out int serialNumber) ? serialNumber : 0;
    }


    public void Use(IEnumerable<Claim> claims, FilterSortObject filterSortObject)
    {
        FilterSortIdentifier filterSortIdentifier = new(GetSerialNumber(claims),
            filterSortObject.TabWindowId, filterSortObject.TableName);
        _filterSortObjects[filterSortIdentifier] = filterSortObject;
    }

    public FilterSortObject? Get(IEnumerable<Claim> claims, int tabWindowId, string tableName)
    {
        FilterSortIdentifier filterSortIdentifier = new(GetSerialNumber(claims), tabWindowId, tableName);
        return _filterSortObjects.TryGetValue(filterSortIdentifier, out var filterSortObject) ? filterSortObject : null;
    }
}