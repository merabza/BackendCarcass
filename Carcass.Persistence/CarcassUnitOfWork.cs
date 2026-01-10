using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carcass.Database;
using DomainShared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Carcass.Persistence;

public /*open*/ class CarcassUnitOfWork : IUnitOfWork
{
    private readonly CarcassDbContext _dbContext;

    public CarcassUnitOfWork(CarcassDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public string GetTableName<T>() where T : class
    {
        var entType = _dbContext.Model.GetEntityTypes().SingleOrDefault(s => s.ClrType == typeof(T));
        return entType?.GetTableName() ?? throw new Exception($"Table Name is null for {typeof(T).Name}");
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}