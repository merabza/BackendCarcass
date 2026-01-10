using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carcass.Database;
using DomainShared.Repositories;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SystemToolsShared.Errors;

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

    public async Task<Option<Err[]>> ExecuteSqlRawRetOptionAsync(string sql,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            return null;
        }
        catch (Exception e)
        {
            return new[] { SystemToolsErrors.UnexpectedDatabaseException(e) };
        }
    }

    public void SetCommandTimeout(TimeSpan timeout)
    {
        _dbContext.Database.SetCommandTimeout(timeout);
    }
}