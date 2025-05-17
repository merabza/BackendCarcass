using CarcassDb.Models;
using System;
using System.Collections.Generic;

namespace CarcassDataSeeding.Comparers;

public class ManyToManyJoinComparer : IEqualityComparer<ManyToManyJoin>
{
    public bool Equals(ManyToManyJoin? x, ManyToManyJoin? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.PtId == y.PtId &&
               x.PKey == y.PKey &&
               x.CtId == y.CtId &&
               x.CKey == y.CKey;
    }

    public int GetHashCode(ManyToManyJoin obj)
    {
        return HashCode.Combine(obj.PtId, obj.PKey, obj.CtId, obj.CKey);
    }
}