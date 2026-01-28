using SystemTools.RepositoriesShared;

namespace BackendCarcass.Database;

public /*open*/ class CarcassUnitOfWork : UnitOfWork
{
    protected CarcassUnitOfWork(CarcassDbContext dbContext) : base(dbContext)
    {
    }
}
