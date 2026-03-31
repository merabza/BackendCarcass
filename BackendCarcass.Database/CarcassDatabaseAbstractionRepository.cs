using SystemTools.RepositoriesShared;

namespace BackendCarcass.Database;

public /*open*/ class CarcassDatabaseAbstractionRepository : DatabaseAbstractionRepository
{
    protected CarcassDatabaseAbstractionRepository(CarcassDbContext dbContext) : base(dbContext)
    {
    }
}
