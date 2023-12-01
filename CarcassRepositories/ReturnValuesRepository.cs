﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CarcassRepositories;

public abstract class ReturnValuesRepository(CarcassDbContext ctx) : IReturnValuesRepository
{

    public async Task<List<DataTypeModelForRvs>> GetDataTypesByTableNames(List<string> tableNames, CancellationToken cancellationToken)
    {
        return await ctx.DataTypes.Where(x => tableNames.Contains(x.DtTable)).Select(x =>
            new DataTypeModelForRvs(x.DtId, x.DtKey, x.DtName, x.DtTable, x.DtIdFieldName, x.DtKeyFieldName,
                x.DtNameFieldName, x.DtParentDataTypeId, x.DtManyToManyJoinParentDataTypeId,
                x.DtManyToManyJoinChildDataTypeId)).ToListAsync(cancellationToken: cancellationToken);
    }

    protected async Task<DataTypeModelForRvs?> GetDataType(int dtId, CancellationToken cancellationToken)
    {
        return await ctx.DataTypes.Where(x => x.DtId == dtId).Select(x => new DataTypeModelForRvs(x.DtId, x.DtKey,
                x.DtName, x.DtTable, x.DtIdFieldName, x.DtKeyFieldName, x.DtNameFieldName, x.DtParentDataTypeId,
                x.DtManyToManyJoinParentDataTypeId, x.DtManyToManyJoinChildDataTypeId))
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private IEntityType? GetEntityTypeByTableName(string tableName)
    {
        return ctx.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
    }

    protected async Task<string?> FindParentFieldName(DataTypeModelForRvs dt, CancellationToken cancellationToken)
    {
        if (dt.DtParentDataTypeId is null)
            return null;

        var parentDataType = await GetDataType(dt.DtParentDataTypeId.Value, cancellationToken);
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



    public abstract Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken);

    public abstract Task<List<SrvModel>> GetAllSimpleReturnValues(DataTypeModelForRvs dt,
        CancellationToken cancellationToken);


}