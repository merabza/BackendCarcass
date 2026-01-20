using BackendCarcass.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCarcass.DatabaseConfiguration;

public sealed class MenuItmConfiguration : IEntityTypeConfiguration<MenuItm>
{
    private const int MenIconNameMaxLength = 50;
    private const int MenKeyMaxLength = 72;
    private const int MenLinkKeyMaxLength = 72;
    private const int MenNameMaxLength = 200;
    private const int MenValueMaxLength = 72;

    public void Configure(EntityTypeBuilder<MenuItm> builder)
    {
        builder.HasKey(e => e.MenId);
        builder.ToTable("Menu");
        builder.HasIndex(e => e.MenKey).IsUnique();

        builder.Property(e => e.MenIconName).HasMaxLength(MenIconNameMaxLength);
        builder.Property(e => e.MenKey).IsRequired().HasMaxLength(MenKeyMaxLength);
        builder.Property(e => e.MenLinkKey).HasMaxLength(MenLinkKeyMaxLength);
        builder.Property(e => e.MenName).IsRequired().HasMaxLength(MenNameMaxLength);
        builder.Property(e => e.MenValue).HasMaxLength(MenValueMaxLength);

        builder.HasOne(d => d.MenGroupNavigation).WithMany(p => p.Menu).HasForeignKey(d => d.MenGroupId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}