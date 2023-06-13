using System;
using System.Collections.Generic;
using CarcassDb.Models;

namespace CarcassDataSeeding;

public interface IDataSeederRepository
{
    bool HaveAnyRecord<T>() where T : class;
    bool CreateEntities<T>(List<T> entities);
    List<T> GetAll<T>() where T : class;
    List<ManyToManyJoin> GetManyToManyJoins(int parentDataTypeId, int childDataTypeId);
    string GetTableName<T>();
    bool SetDtParentDataTypes(Tuple<int, int>[] dtdt);
    bool SetUpdates<T>(List<T> forUpdate);
    bool RemoveRedundantDataTypesByTableNames(string[] toRemoveTableNames);
    bool RemoveNeedlessRecords<TDst>(List<TDst> needLessList) where TDst : class;
    bool SaveChanges();
}