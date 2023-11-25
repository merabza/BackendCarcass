using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;

namespace CarcassRepositories;

public class SqlReturnValuesRepository : ReturnValuesRepository
{
    private readonly CarcassDbContext _ctx;

    public SqlReturnValuesRepository(CarcassDbContext ctx) : base(ctx)
    {
        _ctx = ctx;
    }

    public override async Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModelForRvs dt, CancellationToken cancellationToken)
    {
        string? strSql = null;
        if (dt.DtManyToManyJoinParentDataTypeId is not null && dt.DtManyToManyJoinChildDataTypeId is not null)
        {
            //ეს ის ვარიანტია, როცა მონაცემთა ტიპების წყვილისგან უნდა შედგეს დასაბრუნებელი ინფორმაცია
            var parentDataType = await GetDataType(dt.DtManyToManyJoinParentDataTypeId.Value, cancellationToken);
            var childDataType = await GetDataType(dt.DtManyToManyJoinChildDataTypeId.Value, cancellationToken);

            if (parentDataType is not null && childDataType is not null &&
                IsIdentifier(parentDataType.DtKeyFieldName) && IsIdentifier(childDataType.DtKeyFieldName) &&
                IsIdentifier(parentDataType.DtTable, 32))

                strSql = $"""
                          SELECT
                          	mmjId AS Id,
                          	CONCAT_WS('.', MMJ.PKey, MMJ.CKey) AS [Key],
                          	CONCAT_WS('.', P.{parentDataType.DtKeyFieldName}, C.{childDataType.DtKeyFieldName}) AS [Name]
                          FROM manyToManyJoins MMJ
                          	INNER JOIN {parentDataType.DtTable} P ON MMJ.pKey = P.{parentDataType.DtKeyFieldName}
                          	INNER JOIN {parentDataType.DtTable} C ON MMJ.CKey = C.{childDataType.DtKeyFieldName}
                          WHERE MMJ.PtId = {dt.DtManyToManyJoinParentDataTypeId}
                          	AND MMJ.CtId = {dt.DtManyToManyJoinChildDataTypeId}
                          """;
        }
        else if (IsIdentifier(dt.DtIdFieldName) && IsIdentifier(dt.DtKeyFieldName) && IsIdentifier(dt.DtNameFieldName))
        {
            //ინფორმაციის დაბრუნება უნდა მოხდეს ერთი ცხრილიდან
            var parentFieldName = await FindParentFieldName(dt, cancellationToken);

            strSql =
                $"SELECT {dt.DtIdFieldName} AS value, {dt.DtKeyFieldName} AS [key], {dt.DtNameFieldName} AS [name], {parentFieldName ?? "NULL"} AS parentId FROM {dt.DtTable}";
        }

        if (strSql != null)
            return await _ctx.Set<ReturnValueModel>().FromSqlRaw(strSql).ToListAsync(cancellationToken);
        return new List<ReturnValueModel>();
    }

    private static bool IsIdentifier(string? text, int len = 20)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        return text.Length <= len && text.All(char.IsLetter);
    }
}