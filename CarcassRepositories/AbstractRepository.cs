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

    public IDbContextTransaction GetTransaction()
    {
        return _ctx.Database.BeginTransaction();
    }

    public async Task SaveChangesAsync()
    {
        await _ctx.SaveChangesAsync();
    }
}