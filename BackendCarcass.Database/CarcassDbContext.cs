using BackendCarcassDomain.Entities.AppClaims;
using BackendCarcassDomain.Entities.CrudRightTypes;
using BackendCarcassDomain.Entities.DataTypes;
using BackendCarcassDomain.Entities.ManyToManyJoins;
using BackendCarcassDomain.Entities.MenuGroups;
using BackendCarcassDomain.Entities.MenuItems;
using BackendCarcassDomain.Entities.Roles;
using BackendCarcassDomain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace BackendCarcass.Database;

public /*open*/ class CarcassDbContext : DbContext
{
    public CarcassDbContext(DbContextOptions options, bool isDesignTime) : base(options)
    {
    }

    public CarcassDbContext(DbContextOptions<CarcassDbContext> options, int int1) : base(options)
    {
    }

    public CarcassDbContext(DbContextOptions<CarcassDbContext> options) : base(options)
    {
    }

    public DbSet<AppClaim> AppClaims => Set<AppClaim>();
    public DbSet<DataType> DataTypes => Set<DataType>();
    public DbSet<CrudRightType> CrudRightTypes => Set<CrudRightType>();
    public DbSet<ManyToManyJoin> ManyToManyJoins => Set<ManyToManyJoin>();
    public DbSet<MenuGroup> MenuGroups => Set<MenuGroup>();
    public DbSet<MenuItem> Menu => Set<MenuItem>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarcassDbContext).Assembly);
    }
}
