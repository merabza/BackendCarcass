using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Database;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BackendCarcass.Repositories;

public abstract class ReturnValuesRepository(CarcassDbContext ctx) : IReturnValuesRepository
{
    public Task<List<DataTypeModelForRvs>> GetDataTypesByTableNames(List<string> tableNames,
        CancellationToken cancellationToken = default)
    {
        return ctx.DataTypes.Where(x => tableNames.Contains(x.DtTable)).Select(x =>
                new DataTypeModelForRvs(x.DtId, x.DtTable, x.DtName, x.DtIdFieldName, x.DtKeyFieldName,
                    x.DtNameFieldName,
                    x.DtParentDataTypeId, x.DtManyToManyJoinParentDataTypeId, x.DtManyToManyJoinChildDataTypeId))
            .ToListAsync(cancellationToken);
    }

    public abstract Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModelForRvs dt,
        CancellationToken cancellationToken = default);

    public abstract ValueTask<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt,
        CancellationToken cancellationToken = default);

    protected Task<DataTypeModelForRvs?> GetDataType(int dtId, CancellationToken cancellationToken = default)
    {
        return ctx.DataTypes.Where(x => x.DtId == dtId).Select(x => new DataTypeModelForRvs(x.DtId, x.DtTable, x.DtName,
                x.DtIdFieldName, x.DtKeyFieldName, x.DtNameFieldName, x.DtParentDataTypeId,
                x.DtManyToManyJoinParentDataTypeId, x.DtManyToManyJoinChildDataTypeId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IEntityType? GetEntityTypeByTableName(string tableName)
    {
        return ctx.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
    }

    protected async ValueTask<string?> FindParentFieldName(DataTypeModelForRvs dt,
        CancellationToken cancellationToken = default)
    {
        if (dt.DtParentDataTypeId is null)
        {
            return null;
        }

        DataTypeModelForRvs? parentDataType = await GetDataType(dt.DtParentDataTypeId.Value, cancellationToken);
        if (parentDataType is null)
        {
            return null;
        }

        IEntityType? parentEntType = GetEntityTypeByTableName(parentDataType.DtTable);
        if (parentEntType is null)
        {
            return null;
        }

        List<IKey> parentPrimaryKeys = parentEntType.GetKeys().Where(w => w.IsPrimaryKey()).ToList();
        if (parentPrimaryKeys.Count != 1)
        {
            return null;
        }

        IKey parentPrimaryKey = parentPrimaryKeys[0];
        if (parentPrimaryKey.Properties.Count != 1)
        {
            return null;
        }

        string parentPrimaryKeyFieldName = parentPrimaryKey.Properties[0].Name;

        IEntityType? entType = GetEntityTypeByTableName(dt.DtTable);

        IForeignKey? fn = entType?.GetForeignKeys().SingleOrDefault(x =>
            x.Properties.Count == 1 && x.PrincipalEntityType.Name == parentEntType.Name &&
            x.PrincipalKey.Properties[0].Name == parentPrimaryKeyFieldName);

        return fn?.Properties[0].Name;
    }
}
