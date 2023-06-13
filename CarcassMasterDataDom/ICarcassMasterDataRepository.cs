using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LanguageExt;
using LibCrud;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemToolsShared;

namespace CarcassMasterDataDom;

public interface ICarcassMasterDataRepository : IAbstractRepository
{
    //OneOf<IQueryable<IDataType>, Err[]> LoadByTableName(string tableName);

    object? RunGenericMethodForLoadAllRecords(MethodInfo setMethod, IReadOnlyTypeBase entityType);
    IQueryable<IDataType>? RunGenericMethodForQueryRecords(IReadOnlyTypeBase entityType);

    MethodInfo? MethodInfo();

    IEntityType? GetEntityTypeByTableName(string tableName);
    Task<Option<Err[]>> Create(IDataType newItem);

    string? GetDataTypeGridRulesByTableName(string tableName);
    void Update(IDataType newItem);
    void Delete(IDataType dataType);
}