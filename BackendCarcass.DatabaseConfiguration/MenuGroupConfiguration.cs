using BackendCarcass.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCarcass.DatabaseConfiguration;

public sealed class MenuGroupConfiguration : IEntityTypeConfiguration<MenuGroup>
{
    private const int MengKeyMaxLength = 36;
    private const int MengNameMaxLength = 100;
    private const int MengIconNameMaxLength = 50;

    public void Configure(EntityTypeBuilder<MenuGroup> builder)
    {
        builder.HasKey(e => e.MengId);
        builder.HasIndex(e => e.MengKey).IsUnique();

        builder.Property(e => e.MengKey).IsRequired().HasMaxLength(MengKeyMaxLength);
        builder.Property(e => e.MengName).IsRequired().HasMaxLength(MengNameMaxLength);
        builder.Property(e => e.MengIconName).HasMaxLength(MengIconNameMaxLength);
    }
}
