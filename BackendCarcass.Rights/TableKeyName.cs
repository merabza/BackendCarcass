using System.Threading;
using System.Threading.Tasks;

namespace BackendCarcass.Rights;

public sealed class TableKeyName
{
    public string? TableName { get; init; }

    public async ValueTask<string?> GetTableKey(IUserRightsRepository repo,
        CancellationToken cancellationToken = default)
    {
        if (TableName is not null)
        {
            return await repo.KeyByTableName(TableName, cancellationToken);
        }

        return null;
    }
}
