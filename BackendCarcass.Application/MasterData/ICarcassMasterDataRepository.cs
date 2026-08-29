using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Domain;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.MasterData;

public interface ICarcassMasterDataRepository
{
    //OneOf<IQueryable<IDataType>, ErrorOmd[]> LoadByTableName(string tableName);
    object? RunGenericMethodForLoadAllRecords(MethodInfo setMethod, IReadOnlyTypeBase entityType);

    //IQueryable? RunGenericMethodForQueryRecords(IReadOnlyTypeBase entityType);
    MethodInfo? SetMethodInfo();
    IEntityType? GetEntityTypeByTableName(string tableName);
    Task<Result> Create(IDataType newItem, CancellationToken cancellationToken = default);
    Task<GridModel?> GetDataTypeGridRulesByTableName(string tableName, CancellationToken cancellationToken = default);
    void Update(IDataType newItem);
    void Delete(IDataType dataType);
    Task<string?> GetSortFieldNameByTableName(string tableName, CancellationToken cancellationToken = default);
}
