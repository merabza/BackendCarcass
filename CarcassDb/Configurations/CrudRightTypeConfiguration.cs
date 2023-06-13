using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class CrudRightTypeConfiguration : IEntityTypeConfiguration<CrudRightType>
{
    public void Configure(EntityTypeBuilder<CrudRightType> builder)
    {
        var tableName = nameof(CrudRightType).Pluralize();

        builder.HasKey(e => e.CrtId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.CrtKey).HasDatabaseName(tableName.CreateIndexName(true, nameof(CrudRightType.CrtKey)))
            .IsUnique();
        builder.Property(e => e.CrtId).HasColumnName(nameof(CrudRightType.CrtId).UnCapitalize());
        builder.Property(e => e.CrtKey).IsRequired().HasColumnName(nameof(CrudRightType.CrtKey).UnCapitalize())
            .HasMaxLength(50);
        builder.Property(e => e.CrtName).IsRequired().HasColumnName(nameof(CrudRightType.CrtName).UnCapitalize())
            .HasMaxLength(50);
    }
}