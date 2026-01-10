using Carcass.Database;
using RepositoriesShared;

namespace Carcass.Persistence;

public class CarcassUnitOfWork : UnitOfWork
{
    public CarcassUnitOfWork(CarcassDbContext dbContext) : base(dbContext)
    {
    }
}