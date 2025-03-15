using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class CrudRightTypeConfiguration : IEntityTypeConfiguration<CrudRightType>
{
    public const int CrtKeyMaxLength = 50;
    public const int CrtNameMaxLength = 50;

    public void Configure(EntityTypeBuilder<CrudRightType> builder)
    {
        var tableName = nameof(CrudRightType).Pluralize();
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.CrtId);
        builder.HasIndex(e => e.CrtKey).IsUnique();

        builder.Property(e => e.CrtKey).IsRequired().HasMaxLength(CrtKeyMaxLength);
        builder.Property(e => e.CrtName).IsRequired().HasMaxLength(CrtNameMaxLength);
    }
}