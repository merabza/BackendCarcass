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
        builder.HasIndex(e => e.MengKey).HasDatabaseName(tableName.CreateIndexName(true, nameof(MenuGroup.MengKey)))
            .IsUnique();
        builder.Property(e => e.MengId).HasColumnName(nameof(MenuGroup.MengId).UnCapitalize());
        builder.Property(e => e.MengKey).IsRequired().HasColumnName(nameof(MenuGroup.MengKey).UnCapitalize())
            .HasMaxLength(36);
        builder.Property(e => e.MengName).IsRequired().HasColumnName(nameof(MenuGroup.MengName).UnCapitalize())
            .HasMaxLength(100);
        builder.Property(e => e.SortId).HasColumnName(nameof(MenuGroup.SortId).UnCapitalize());
        builder.Property(e => e.MengIconName).HasColumnName(nameof(MenuGroup.MengIconName).UnCapitalize());
        builder.Property(e => e.Hidden).HasColumnName(nameof(MenuGroup.Hidden).UnCapitalize());
    }
}