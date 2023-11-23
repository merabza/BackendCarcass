using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarcassDb;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CarcassRepositories;

public abstract class ReturnValuesRepository(CarcassDbContext ctx) : IReturnValuesRepository
{
    
    public async Task<List<DataTypeModel>> GetDataTypesByTableNames(List<string> tableNames)
    {
        return await ctx.DataTypes.Where(x => tableNames.Contains(x.DtTable)).Select(x => new DataTypeModel(x.DtId, x.DtKey,
            x.DtName, x.DtNameNominative, x.DtNameGenitive, x.DtTable, x.DtIdFieldName, x.DtKeyFieldName,
            x.DtNameFieldName, x.DtParentDataTypeId, x.DtManyToManyJoinParentDataTypeId,
            x.DtManyToManyJoinChildDataTypeId)).ToListAsync();
    }

    
    public async Task<DataTypeModel?> GetDataType(int dtId)
    {
        return await ctx.DataTypes.Where(x => x.DtId == dtId).Select(x => new DataTypeModel(x.DtId, x.DtKey,
            x.DtName, x.DtNameNominative, x.DtNameGenitive, x.DtTable, x.DtIdFieldName, x.DtKeyFieldName,
            x.DtNameFieldName, x.DtParentDataTypeId, x.DtManyToManyJoinParentDataTypeId,
            x.DtManyToManyJoinChildDataTypeId)).SingleOrDefaultAsync();
    }
    
    public IEntityType? GetEntityTypeByTableName(string tableName)
    {
        return ctx.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
    }

    protected async Task<string?> FindParentFieldName(DataTypeModel dt)
    {
        if (dt.DtParentDataTypeId is null)
            return null;

        var parentDataType = await GetDataType(dt.DtParentDataTypeId.Value);
        if (parentDataType is null)
            return null;

        var parentEntType = GetEntityTypeByTableName(parentDataType.DtTable);
        if (parentEntType is null)
            return null;

        var parentPrimaryKeys = parentEntType.GetKeys().Where(w => w.IsPrimaryKey()).ToList();
        if (parentPrimaryKeys.Count != 1)
            return null;

        var parentPrimaryKey = parentPrimaryKeys[0];
        if (parentPrimaryKey.Properties.Count != 1)
            return null;

        var parentPrimaryKeyFieldName = parentPrimaryKey.Properties[0].Name;


        var entType = GetEntityTypeByTableName(dt.DtTable);

        var fn = entType?.GetForeignKeys().SingleOrDefault(x =>
            x.Properties.Count == 1 && x.PrincipalEntityType.Name == parentEntType.Name &&
            x.PrincipalKey.Properties[0].Name == parentPrimaryKeyFieldName);

        return fn?.Properties[0].Name;

    }



    public abstract Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModel dt);

}