using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application;
using BackendCarcass.Domain.AppClaims;
using BackendCarcass.Domain.CrudRightTypes;
using BackendCarcass.Domain.DataTypes;
using BackendCarcass.Domain.ManyToManyJoins;
using BackendCarcass.Domain.MenuGroups;
using BackendCarcass.Domain.MenuItems;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemTools.SharedKernel;

namespace BackendCarcass.Database;

public /*open*/ class CarcassDbContext : DbContext, ICarcassApplicationDbContext
{
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public CarcassDbContext(DbContextOptions options, bool isDesignTime) : base(options)
    {
    }

    public CarcassDbContext(DbContextOptions<CarcassDbContext> options, int int1) : base(options)
    {
    }

    public CarcassDbContext(DbContextOptions<CarcassDbContext> options, IDomainEventsDispatcher domainEventsDispatcher)
        : base(options)
    {
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    public DbSet<AppClaim> AppClaims => Set<AppClaim>();
    public DbSet<DataType> DataTypes => Set<DataType>();
    public DbSet<CrudRightType> CrudRightTypes => Set<CrudRightType>();
    public DbSet<ManyToManyJoin> ManyToManyJoins => Set<ManyToManyJoin>();
    public DbSet<MenuGroup> MenuGroups => Set<MenuGroup>();
    public DbSet<MenuItem> Menu => Set<MenuItem>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    public IEntityType? GetEntityTypeByTableName(string tableName)
    {
        return Model.GetEntityTypes().SingleOrDefault(w => w.GetTableName() == tableName);
    }

    public string GetTableName<T>() where T : class
    {
        IEntityType? entType = Model.GetEntityTypes().SingleOrDefault(s => s.ClrType == typeof(T));
        return entType?.GetTableName() ?? throw new Exception($"Table Name is null for {typeof(T).Name}");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarcassDbContext).Assembly);
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker.Entries<Entity>().Select(entry => entry.Entity).SelectMany(entity =>
        {
            List<IDomainEvent> domainEvents = entity.DomainEvents;

            entity.ClearDomainEvents();

            return domainEvents;
        }).ToList();

        await _domainEventsDispatcher.DispatchAsync(domainEvents);
    }
}
