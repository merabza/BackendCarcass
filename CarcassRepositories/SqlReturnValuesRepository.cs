﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace CarcassRepositories;

public sealed class SqlReturnValuesRepository(CarcassDbContext ctx) : ReturnValuesRepository(ctx)
{
    private readonly CarcassDbContext _ctx = ctx;

    public override async ValueTask<List<SrvModel>> GetSimpleReturnValues(DataTypeModelForRvs dt,
        CancellationToken cancellationToken = default)
    {
        string? strSql = null;
        if (IsIdentifier(dt.DtIdFieldName) && (dt.DtKeyFieldName is null || IsIdentifier(dt.DtKeyFieldName)) &&
            (dt.DtNameFieldName is null || IsIdentifier(dt.DtNameFieldName)))
            //ინფორმაციის დაბრუნება უნდა მოხდეს ერთი ცხრილიდან
            strSql =
                $"SELECT {dt.DtIdFieldName} AS id, {dt.DtNameFieldName ?? dt.DtKeyFieldName ?? "NULL"} AS [name] FROM {dt.DtTable}";

        if (strSql != null)
            return await _ctx.Set<SrvModel>().FromSqlRaw(strSql).ToListAsync(cancellationToken);
        return [];
    }

    public override async Task<List<ReturnValueModel>> GetAllReturnValues(DataTypeModelForRvs dt,
        CancellationToken cancellationToken = default)
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
                          	CONCAT_WS('.', P.{parentDataType.DtNameFieldName}, C.{childDataType.DtNameFieldName}) AS [Name],
                          	NULL AS ParentId
                          FROM ManyToManyJoins MMJ
                          	INNER JOIN {parentDataType.DtTable} P ON MMJ.pKey = P.{parentDataType.DtKeyFieldName}
                          	INNER JOIN {childDataType.DtTable} C ON MMJ.CKey = C.{childDataType.DtKeyFieldName}
                          WHERE MMJ.PtId = {dt.DtManyToManyJoinParentDataTypeId}
                          	AND MMJ.CtId = {dt.DtManyToManyJoinChildDataTypeId}
                          """;
        }
        else if (IsIdentifier(dt.DtIdFieldName) && (dt.DtKeyFieldName is null || IsIdentifier(dt.DtKeyFieldName)) &&
                 (dt.DtNameFieldName is null || IsIdentifier(dt.DtNameFieldName)))
        {
            //ინფორმაციის დაბრუნება უნდა მოხდეს ერთი ცხრილიდან
            var parentFieldName = await FindParentFieldName(dt, cancellationToken);

            strSql =
                $"SELECT {dt.DtIdFieldName} AS id, {dt.DtKeyFieldName ?? "NULL"} AS [key], {dt.DtNameFieldName ?? "NULL"} AS [name], {parentFieldName ?? "NULL"} AS parentId FROM {dt.DtTable}";
        }

        if (strSql != null)
            return await _ctx.Set<ReturnValueModel>().FromSqlRaw(strSql).ToListAsync(cancellationToken);
        return [];
    }

    private static bool IsIdentifier(string? text, int len = 20)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        return text.Length <= len && text.All(char.IsLetter);
    }
}