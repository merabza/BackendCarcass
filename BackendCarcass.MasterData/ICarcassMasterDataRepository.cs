using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.MasterData;

public interface ICarcassMasterDataRepository
{
    //OneOf<IQueryable<IDataType>, Err[]> LoadByTableName(string tableName);
    object? RunGenericMethodForLoadAllRecords(MethodInfo setMethod, IReadOnlyTypeBase entityType);

    //IQueryable? RunGenericMethodForQueryRecords(IReadOnlyTypeBase entityType);
    MethodInfo? SetMethodInfo();
    IEntityType? GetEntityTypeByTableName(string tableName);
    Task<Option<Err[]>> Create(IDataType newItem, CancellationToken cancellationToken = default);
    Task<GridModel?> GetDataTypeGridRulesByTableName(string tableName, CancellationToken cancellationToken = default);
    void Update(IDataType newItem);
    void Delete(IDataType dataType);
    Task<string?> GetSortFieldNameByTableName(string tableName, CancellationToken cancellationToken = default);
}
