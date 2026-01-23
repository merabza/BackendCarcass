using System;
using System.Collections.Generic;
using BackendCarcass.Database.Models;

namespace BackendCarcass.DataSeeding.Comparers;

public class ManyToManyJoinComparer : IEqualityComparer<ManyToManyJoin>
{
    public bool Equals(ManyToManyJoin? x, ManyToManyJoin? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.PtId == y.PtId && x.PKey.Equals(y.PKey, StringComparison.OrdinalIgnoreCase) && x.CtId == y.CtId &&
               x.CKey.Equals(y.CKey, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(ManyToManyJoin obj)
    {
        return HashCode.Combine(obj.PtId, obj.PKey, obj.CtId, obj.CKey);
    }
}
