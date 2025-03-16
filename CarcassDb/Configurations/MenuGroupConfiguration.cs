using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class MenuGroupConfiguration : IEntityTypeConfiguration<MenuGroup>
{
    public const int MengKeyMaxLength = 36;
    public const int MengNameMaxLength = 100;
    public const int MengIconNameMaxLength = 50;

    public void Configure(EntityTypeBuilder<MenuGroup> builder)
    {
        //var tableName = nameof(MenuGroup).Pluralize();
        //builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.MengId);
        builder.HasIndex(e => e.MengKey).IsUnique();

        builder.Property(e => e.MengKey).IsRequired().HasMaxLength(MengKeyMaxLength);
        builder.Property(e => e.MengName).IsRequired().HasMaxLength(MengNameMaxLength);
        builder.Property(e => e.MengIconName).HasMaxLength(MengIconNameMaxLength);
    }
}