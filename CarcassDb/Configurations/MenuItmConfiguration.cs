using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class MenuItmConfiguration : IEntityTypeConfiguration<MenuItm>
{
    public void Configure(EntityTypeBuilder<MenuItm> builder)
    {
        var tableName = nameof(MenuItm).Pluralize();

        builder.HasKey(e => e.MenId);
        builder.ToTable("menu");
        builder.HasIndex(e => e.MenKey).HasDatabaseName(tableName.CreateIndexName(true, nameof(MenuItm.MenKey)))
            .IsUnique();
        builder.Property(e => e.MenId).HasColumnName(nameof(MenuItm.MenId).UnCapitalize());
        builder.Property(e => e.MenGroupId).HasColumnName(nameof(MenuItm.MenGroupId).UnCapitalize());

        builder.Property(e => e.MenIconName).HasColumnName(nameof(MenuItm.MenIconName).UnCapitalize());
        builder.Property(e => e.MenKey).IsRequired().HasColumnName(nameof(MenuItm.MenKey).UnCapitalize())
            .HasMaxLength(72);
        builder.Property(e => e.MenLinkKey).HasColumnName(nameof(MenuItm.MenLinkKey).UnCapitalize()).HasMaxLength(72);
        builder.Property(e => e.MenName).IsRequired().HasColumnName(nameof(MenuItm.MenName).UnCapitalize())
            .HasMaxLength(200);
        builder.Property(e => e.SortId).HasColumnName(nameof(MenuItm.SortId).UnCapitalize());
        builder.Property(e => e.MenValue).HasColumnName(nameof(MenuItm.MenValue).UnCapitalize()).HasMaxLength(72);
        builder.HasOne(d => d.MenGroupNavigation).WithMany(p => p.Menu).HasForeignKey(d => d.MenGroupId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName(tableName.CreateConstraintName(nameof(MenuGroup)));
    }
}