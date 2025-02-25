using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class MenuItmConfiguration : IEntityTypeConfiguration<MenuItm>
{
    public void Configure(EntityTypeBuilder<MenuItm> builder)
    {
        builder.HasKey(e => e.MenId);
        builder.ToTable("menu");
        builder.HasIndex(e => e.MenKey).IsUnique();

        builder.Property(e => e.MenIconName).HasMaxLength(50);
        builder.Property(e => e.MenKey).IsRequired().HasMaxLength(72);
        builder.Property(e => e.MenLinkKey).HasMaxLength(72);
        builder.Property(e => e.MenName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.MenValue).HasMaxLength(72);

        builder.HasOne(d => d.MenGroupNavigation).WithMany(p => p.Menu).HasForeignKey(d => d.MenGroupId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}