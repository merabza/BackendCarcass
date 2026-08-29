using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData;
using BackendCarcass.Domain;
using BackendCarcass.Domain.DataTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Repositories;

public /*open*/ class CarcassMasterDataRepository : ICarcassMasterDataRepository
{
    private readonly ICarcassApplicationDbContext _context;

    protected CarcassMasterDataRepository(ICarcassApplicationDbContext carcassContext)
    {
        _context = carcassContext;
    }

//public OneOf<IQueryable<IDataType>, Err[]> LoadByTableName(string tableName)
//{
//    var vvv = GetEntityTypeByTableName(tableName);// _context.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
//    if (vvv == null)
//        return new[] { MasterDataApiErrors.TableNotFound(tableName) }; //ვერ ვიპოვეთ შესაბამისი ცხრილი

//    var setMethod = MethodInfo();
//    if (setMethod == null)
//        return new[] { MasterDataApiErrors.SetMethodNotFoundForTable(tableName) }; //ცხრილს არ აქვს მეთოდი Set

//    var result = MakeGenericMethod(setMethod, vvv);
//    return result == null
//        ? new[]
//        {
//            MasterDataApiErrors.SetMethodReturnsNullForTable(tableName)
//        } //ცხრილის Set მეთოდი აბრუნებს null-ს
//        : OneOf<IQueryable<IDataType>, Err[]>.FromT0((IQueryable<IDataType>)result);
//}

    public object? RunGenericMethodForLoadAllRecords(MethodInfo setMethod, IReadOnlyTypeBase entityType)
    {
        return setMethod.MakeGenericMethod(entityType.ClrType).Invoke(_context, null);
    }

//public IQueryable? RunGenericMethodForQueryRecords(IReadOnlyTypeBase entityType)
//{
//    return (IQueryable?)_context.GetType().GetMethod("Set")?.MakeGenericMethod(entityType.ClrType)
//        .Invoke(_context, null);
//}

    public MethodInfo? SetMethodInfo()
    {
        return _context.GetType().GetMethod("Set", []);
    }

    public IEntityType? GetEntityTypeByTableName(string tableName)
    {
        return _context.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
    }

    public async Task<GridModel?> GetDataTypeGridRulesByTableName(string tableName,
        CancellationToken cancellationToken = default)
    {
        DataType? dataType =
            await _context.DataTypes.SingleOrDefaultAsync(s => s.DtTable == tableName, cancellationToken);
        string? dtGridRulesJson = dataType?.DtGridRulesJson;

        return dtGridRulesJson == null ? null : GridModel.DeserializeGridModel(dtGridRulesJson);
    }

    public void Update(IDataType newItem)
    {
        _context.Update(newItem);
    }

    public void Delete(IDataType dataType)
    {
        _context.Remove(dataType);
    }

    public async Task<string?> GetSortFieldNameByTableName(string tableName,
        CancellationToken cancellationToken = default)
    {
        DataType? dataType =
            await _context.DataTypes.SingleOrDefaultAsync(s => s.DtTable == tableName, cancellationToken);
        return dataType?.DtNameFieldName;
    }

    public async Task<Result> Create(IDataType newItem, CancellationToken cancellationToken = default)
    {
        await _context.AddAsync(newItem, cancellationToken);
        //await _context.SaveChangesAsync();
        return Result.Success();
    }
}
