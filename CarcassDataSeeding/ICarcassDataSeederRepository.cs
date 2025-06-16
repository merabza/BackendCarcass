using System;
using System.Collections.Generic;
using CarcassDb.Models;
using RepositoriesDom;

namespace CarcassDataSeeding;

public interface ICarcassDataSeederRepository : IAbstractRepository
{
    List<ManyToManyJoin> GetManyToManyJoins(int parentDataTypeId, int childDataTypeId);
    bool SetDtParentDataTypes(Tuple<int, int>[] dtdt);
    bool SetManyToManyJoinParentChildDataTypes(Tuple<int, int, int>[] dtdtdt);
    bool RemoveRedundantDataTypesByTableNames(string[] toRemoveTableNames);
}