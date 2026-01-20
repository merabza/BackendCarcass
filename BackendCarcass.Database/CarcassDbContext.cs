using BackendCarcass.Database.Models;
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
    public DbSet<MenuItm> Menu => Set<MenuItm>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarcassDbContext).Assembly);
    }
}
