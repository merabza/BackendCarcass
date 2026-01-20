using BackendCarcass.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCarcass.DatabaseConfiguration;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private const int RolKeyMaxLength = 256;
    private const int RolLevelMaxLength = 1000;
    private const int RolNameMaxLength = 100;
    private const int RolNormalizedKeyMaxLength = 256;

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.RolId);
        builder.HasIndex(e => e.RolKey).IsUnique();

        builder.Property(e => e.RolKey).IsRequired().HasMaxLength(RolKeyMaxLength);
        builder.Property(e => e.RolLevel).IsRequired().HasDefaultValue(RolLevelMaxLength);
        builder.Property(e => e.RolName).IsRequired().HasMaxLength(RolNameMaxLength);
        builder.Property(e => e.RolNormalizedKey).IsRequired().HasMaxLength(RolNormalizedKeyMaxLength);
    }
}
