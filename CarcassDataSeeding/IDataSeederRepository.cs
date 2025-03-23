using System;
using System.Collections.Generic;
using CarcassDb.Models;

namespace CarcassDataSeeding;

public interface IDataSeederRepository
{
    bool HaveAnyRecord<T>() where T : class;
    bool CreateEntities<T>(List<T> entities) where T : class;
    bool DeleteEntities<T>(List<T> entities) where T : class;
    List<T> GetAll<T>() where T : class;
    List<ManyToManyJoin> GetManyToManyJoins(int parentDataTypeId, int childDataTypeId);
    string GetTableName<T>() where T : class;
    bool SetDtParentDataTypes(Tuple<int, int>[] dtdt);
    bool SetManyToManyJoinParentChildDataTypes(Tuple<int, int, int>[] dtdtdt);
    bool SetUpdates<T>(List<T> forUpdate) where T : class;
    bool RemoveRedundantDataTypesByTableNames(string[] toRemoveTableNames);
    bool RemoveNeedlessRecords<T>(List<T> needLessList) where T : class;
    bool SaveChanges();
}