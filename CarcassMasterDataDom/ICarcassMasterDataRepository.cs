using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CarcassMasterDataDom.Models;
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
    Task<Option<Err[]>> Create(IDataType newItem, CancellationToken cancellationToken);

    Task<string?> GetDataTypeGridRulesByTableName(string tableName, CancellationToken cancellationToken);
    void Update(IDataType newItem);
    void Delete(IDataType dataType);


}