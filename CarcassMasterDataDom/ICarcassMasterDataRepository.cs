using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore.Metadata;
using RepositoriesDom;
using SystemToolsShared.Errors;

namespace CarcassMasterDataDom;

public interface ICarcassMasterDataRepository : IAbstractRepository
{
    //OneOf<IQueryable<IDataType>, IEnumerable<Err>> LoadByTableName(string tableName);
    object? RunGenericMethodForLoadAllRecords(MethodInfo setMethod, IReadOnlyTypeBase entityType);

    //IQueryable? RunGenericMethodForQueryRecords(IReadOnlyTypeBase entityType);
    MethodInfo? SetMethodInfo();
    IEntityType? GetEntityTypeByTableName(string tableName);
    Task<Option<IEnumerable<Err>>> Create(IDataType newItem, CancellationToken cancellationToken = default);
    Task<GridModel?> GetDataTypeGridRulesByTableName(string tableName, CancellationToken cancellationToken = default);
    void Update(IDataType newItem);
    void Delete(IDataType dataType);
    Task<string?> GetSortFieldNameByTableName(string tableName, CancellationToken cancellationToken = default);
}