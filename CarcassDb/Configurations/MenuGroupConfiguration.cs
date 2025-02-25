using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class MenuGroupConfiguration : IEntityTypeConfiguration<MenuGroup>
{
    public void Configure(EntityTypeBuilder<MenuGroup> builder)
    {
        var tableName = nameof(MenuGroup).Pluralize();

        builder.HasKey(e => e.MengId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.MengKey).IsUnique();
        builder.Property(e => e.MengKey).IsRequired().HasMaxLength(36);
        builder.Property(e => e.MengName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MengIconName).HasMaxLength(50);
    }
}