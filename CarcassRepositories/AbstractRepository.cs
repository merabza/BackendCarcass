using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibCrud;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CarcassRepositories;

public /*open*/ class AbstractRepository : IAbstractRepository
{
    private readonly DbContext _ctx;

    protected AbstractRepository(DbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IDbContextTransaction> GetTransaction(CancellationToken cancellationToken)
    {
        return await _ctx.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _ctx.SaveChangesAsync(cancellationToken);
    }

    public string? GetTableName<T>()
    {
        var entType = _ctx.Model.GetEntityTypes().SingleOrDefault(s => s.ClrType == typeof(T));
        return entType?.GetTableName();
    }
}