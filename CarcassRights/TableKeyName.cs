﻿using System.Threading;
using System.Threading.Tasks;
using CarcassDom;

namespace CarcassRights;

public sealed class TableKeyName
{
    public string? TableKey { get; init; }
    public string? TableName { get; init; }

    public async ValueTask<string?> GetTableKey(IUserRightsRepository repo,
        CancellationToken cancellationToken = default)
    {
        if (TableKey is not null)
            return TableKey;
        if (TableName is not null)
            return await repo.KeyByTableName(TableName, cancellationToken);
        return null;
    }
}