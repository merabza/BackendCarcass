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
        builder.HasIndex(e => e.RolKey).HasDatabaseName(tableName.CreateIndexName(true, nameof(Role.RolKey)))
            .IsUnique();
        builder.Property(e => e.RolId).HasColumnName(nameof(Role.RolId).UnCapitalize());
        builder.Property(e => e.RolKey).IsRequired().HasColumnName(nameof(Role.RolKey).UnCapitalize())
            .HasMaxLength(256);
        builder.Property(e => e.RolLevel).IsRequired().HasColumnName(nameof(Role.RolLevel).UnCapitalize())
            .HasDefaultValue(1000);
        builder.Property(e => e.RolName).IsRequired().HasColumnName(nameof(Role.RolName).UnCapitalize())
            .HasMaxLength(100);
        builder.Property(e => e.RolNormalizedKey).IsRequired()
            .HasColumnName(nameof(Role.RolNormalizedKey).UnCapitalize()).HasMaxLength(256);
    }
}