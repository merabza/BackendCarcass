using BackendCarcass.Database;
using SystemTools.RepositoriesShared;

namespace BackendCarcass.Persistence;

public class CarcassUnitOfWork : UnitOfWork
{
    public CarcassUnitOfWork(CarcassDbContext dbContext) : base(dbContext)
    {
    }
}
