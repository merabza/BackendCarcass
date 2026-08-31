using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Domain.AppClaims;
using BackendCarcass.Domain.CrudRightTypes;
using BackendCarcass.Domain.DataTypes;
using BackendCarcass.Domain.ManyToManyJoins;
using BackendCarcass.Domain.MenuGroups;
using BackendCarcass.Domain.MenuItems;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application;

public interface ICarcassApplicationDbContext
{
    DbSet<AppClaim> AppClaims { get; }
    DbSet<DataType> DataTypes { get; }
    DbSet<CrudRightType> CrudRightTypes { get; }
    DbSet<ManyToManyJoin> ManyToManyJoins { get; }
    DbSet<MenuGroup> MenuGroups { get; }
    DbSet<MenuItem> Menu { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    IModel Model { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    string GetTableName<T>() where T : class;
    IEntityType? GetEntityTypeByTableName(string tableName);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default);
    EntityEntry Update(object entity);
    EntityEntry Remove(object entity);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    void Detach(Entity entity);

    //Task<Option<ErrorOmd[]>> ExecuteSqlRawRetOptionAsync(string sql, CancellationToken cancellationToken = default);
    //void SetCommandTimeout(TimeSpan timeout);
}
