using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        var tableName = nameof(Role).Pluralize();

        builder.HasKey(e => e.RolId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.RolKey).IsUnique();
        builder.Property(e => e.RolKey).IsRequired().HasMaxLength(256);
        builder.Property(e => e.RolLevel).IsRequired().HasDefaultValue(1000);
        builder.Property(e => e.RolName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.RolNormalizedKey).IsRequired().HasMaxLength(256);
    }
}