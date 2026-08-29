using BackendCarcass.Application;

namespace BackendCarcass.Database;

public /*open*/ class CarcassDatabaseAbstractionRepository
{
    protected CarcassDatabaseAbstractionRepository(ICarcassApplicationDbContext dbContext)
    {
    }
}
