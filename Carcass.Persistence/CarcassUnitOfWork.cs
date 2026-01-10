using Carcass.Database;
using DomainShared;

namespace Carcass.Persistence;

public /*open*/ class CarcassUnitOfWork(CarcassDbContext ctx) : UnitOfWork(ctx);