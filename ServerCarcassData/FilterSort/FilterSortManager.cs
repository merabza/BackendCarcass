using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ServerCarcassData.Domain;

namespace ServerCarcassData.FilterSort;

public sealed class FilterSortManager
{
    //private static FilterSortManager _instance;
    //private static readonly object SyncRoot = new Object();
    //public static FilterSortManager Instance
    //{
    //  get
    //  {
    //    if (_instance == null)
    //    {
    //      lock (SyncRoot) //thread safe singleton
    //      {
    //        if (_instance == null)
    //          _instance = new FilterSortManager();
    //      }
    //    }
    //    return _instance;
    //  }
    //}


    //private readonly List<FilterSortObject> _filterSortObjects = new List<FilterSortObject>();
    private readonly Dictionary<FilterSortIdentifier, FilterSortObject> _filterSortObjects =
        new(new FilterSortIdentifierEqualityComparer());

    private static int SerialNumber(IEnumerable<Claim> claims)
    {
        var serialNumberClaim = claims.SingleOrDefault(so => so.Type == ClaimTypes.SerialNumber);
        if (serialNumberClaim == null)
            return 0;
        var strSerialNumber = serialNumberClaim.Value;
        return int.TryParse(strSerialNumber, out var serialNumber) ? serialNumber : 0;
    }


    public void Use(IEnumerable<Claim> claims, FilterSortObject filterSortObject)
    {
        FilterSortIdentifier filterSortIdentifier = new(SerialNumber(claims),
            filterSortObject.TabWindowId, filterSortObject.TableName);
        if (_filterSortObjects.ContainsKey(filterSortIdentifier))
            _filterSortObjects[filterSortIdentifier] = filterSortObject;
        else
            _filterSortObjects.Add(filterSortIdentifier, filterSortObject);
    }

    public FilterSortObject? Get(IEnumerable<Claim> claims, int tabWindowId, string tableName)
    {
        FilterSortIdentifier filterSortIdentifier = new(SerialNumber(claims), tabWindowId, tableName);
        return _filterSortObjects.ContainsKey(filterSortIdentifier) ? _filterSortObjects[filterSortIdentifier] : null;
    }
}