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
        builder.HasIndex(e => e.CrtKey).IsUnique();
        builder.Property(e => e.CrtKey).IsRequired().HasMaxLength(50);
        builder.Property(e => e.CrtName).IsRequired().HasMaxLength(50);
    }
}