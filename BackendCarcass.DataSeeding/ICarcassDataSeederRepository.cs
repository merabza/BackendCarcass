using System;
using System.Collections.Generic;
using BackendCarcass.Database.Models;

namespace BackendCarcass.DataSeeding;

public interface ICarcassDataSeederRepository
{
    List<ManyToManyJoin> GetManyToManyJoins(int parentDataTypeId, int childDataTypeId);
    bool SetDtParentDataTypes(Tuple<int, int>[] dtdt);
    bool SetManyToManyJoinParentChildDataTypes(Tuple<int, int, int>[] dtdtdt);
    bool RemoveRedundantDataTypesByTableNames(string[] toRemoveTableNames);
}
