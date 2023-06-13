using System.Threading.Tasks;
using CarcassRightsDom;

namespace CarcassRights;

public class TableKeyName
{
    public string? TableKey { get; init; }
    public string? TableName { get; init; }


    public async Task<string?> GetTableKey(IUserRightsRepository repo)
    {
        if (TableKey is not null)
            return TableKey;
        if (TableName is not null)
            return await repo.KeyByTableName(TableName);
        return null;
    }
}