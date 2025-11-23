using System;
using System.Collections.Generic;
using CarcassDb.Models;

namespace CarcassDataSeeding.Comparers;

public class ManyToManyJoinComparer : IEqualityComparer<ManyToManyJoin>
{
    public bool Equals(ManyToManyJoin? x, ManyToManyJoin? y)
    {
        if (x == null || y == null) return false;

        return x.PtId == y.PtId && x.PKey.Equals(y.PKey, StringComparison.CurrentCultureIgnoreCase) &&
               x.CtId == y.CtId && x.CKey.Equals(y.CKey, StringComparison.CurrentCultureIgnoreCase);
    }

    public int GetHashCode(ManyToManyJoin obj)
    {
        return HashCode.Combine(obj.PtId, obj.PKey, obj.CtId, obj.CKey);
    }
}