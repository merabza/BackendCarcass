using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb;
using CarcassMasterDataDom;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemToolsShared;

namespace CarcassRepositories;

public class CarcassMasterDataRepository : AbstractRepository, ICarcassMasterDataRepository
{
    private readonly CarcassDbContext _context;

    protected CarcassMasterDataRepository(CarcassDbContext carcassContext) : base(carcassContext)
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

    public IQueryable? RunGenericMethodForQueryRecords(IReadOnlyTypeBase entityType)
    {
        return (IQueryable?)_context.GetType().GetMethod("Set")?.MakeGenericMethod(entityType.ClrType)
            .Invoke(_context, null);
    }

    public MethodInfo? SetMethodInfo()
    {
        return _context.GetType().GetMethod("Set", []);
    }

    public IEntityType? GetEntityTypeByTableName(string tableName)
    {
        return _context.Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
    }


    public async Task<Option<Err[]>> Create(IDataType newItem, CancellationToken cancellationToken)
    {
        await _context.AddAsync(newItem, cancellationToken);
        //await _context.SaveChangesAsync();
        return null;
    }

    public async Task<GridModel?> GetDataTypeGridRulesByTableName(string tableName, CancellationToken cancellationToken)
    {
        var dataType = await _context.DataTypes.SingleOrDefaultAsync(s => s.DtTable == tableName, cancellationToken);
        var dtGridRulesJson = dataType?.DtGridRulesJson;

        return dtGridRulesJson == null ? null : GridModel.DeserializeGridModel(dtGridRulesJson);
    }

    public void Update(IDataType newItem)
    {
        _context.Update(newItem);
    }

    public void Delete(IDataType itemForDelete)
    {
        _context.Remove(itemForDelete);
    }
}