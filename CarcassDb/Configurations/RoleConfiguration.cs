using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public const int RolKeyMaxLength = 256;
    public const int RolLevelMaxLength = 1000;
    public const int RolNameMaxLength = 100;
    public const int RolNormalizedKeyMaxLength = 256;

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        var tableName = nameof(Role).Pluralize();
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.RolId);
        builder.HasIndex(e => e.RolKey).IsUnique();

        builder.Property(e => e.RolKey).IsRequired().HasMaxLength(RolKeyMaxLength);
        builder.Property(e => e.RolLevel).IsRequired().HasDefaultValue(RolLevelMaxLength);
        builder.Property(e => e.RolName).IsRequired().HasMaxLength(RolNameMaxLength);
        builder.Property(e => e.RolNormalizedKey).IsRequired().HasMaxLength(RolNormalizedKeyMaxLength);
    }
}