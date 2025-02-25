using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class MenuGroupConfiguration : IEntityTypeConfiguration<MenuGroup>
{
    public void Configure(EntityTypeBuilder<MenuGroup> builder)
    {
        builder.HasKey(e => e.MengId);
        builder.HasIndex(e => e.MengKey).IsUnique();

        builder.Property(e => e.MengKey).IsRequired().HasMaxLength(36);
        builder.Property(e => e.MengName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MengIconName).HasMaxLength(50);
    }
}