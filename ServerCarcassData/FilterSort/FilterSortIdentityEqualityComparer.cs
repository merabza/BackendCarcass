using System.Collections.Generic;

namespace ServerCarcassData.FilterSort;

public sealed class FilterSortIdentifierEqualityComparer : IEqualityComparer<FilterSortIdentifier>
{
    public bool Equals(FilterSortIdentifier? x, FilterSortIdentifier? y)
    {
        //First check if both object reference are equal then return true
        if (ReferenceEquals(x, y)) return true;
        //If either one of the object reference is null, return false
        if (x is null || y is null) return false;
        //Comparing all the properties one by one
        return x.UserSerialNumber == y.UserSerialNumber && x.TabWindowId == y.TabWindowId &&
               x.TableName == y.TableName;
    }

    public int GetHashCode(FilterSortIdentifier obj)
    {
        return obj.UserSerialNumber.GetHashCode() ^ obj.TabWindowId.GetHashCode() ^
               DeterministicHashCode(obj.TableName);
    }

    private static int DeterministicHashCode(string str)
    {
        unchecked
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;

            for (var i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + hash2 * 1566083941;
        }
    }
}