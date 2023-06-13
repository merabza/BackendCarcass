using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class DataTypeConfiguration : IEntityTypeConfiguration<DataType>
{
    public void Configure(EntityTypeBuilder<DataType> builder)
    {
        var tableName = nameof(DataType).Pluralize();

        builder.HasKey(e => e.DtId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.DtKey).HasDatabaseName(tableName.CreateIndexName(true, nameof(DataType.DtKey)))
            .IsUnique();
        builder.HasIndex(e => e.DtTable).HasDatabaseName(tableName.CreateIndexName(true, nameof(DataType.DtTable)))
            .IsUnique();
        builder.Property(e => e.DtId).HasColumnName(nameof(DataType.DtId).UnCapitalize());
        builder.Property(e => e.DtKey).IsRequired().HasColumnName(nameof(DataType.DtKey).UnCapitalize())
            .HasMaxLength(36);
        builder.Property(e => e.DtName).IsRequired().HasColumnName(nameof(DataType.DtName).UnCapitalize())
            .HasMaxLength(100);
        builder.Property(e => e.DtNameNominative).IsRequired()
            .HasColumnName(nameof(DataType.DtNameNominative).UnCapitalize()).HasMaxLength(100);
        builder.Property(e => e.DtNameGenitive).IsRequired()
            .HasColumnName(nameof(DataType.DtNameGenitive).UnCapitalize()).HasMaxLength(100);
        builder.Property(e => e.DtTable).IsRequired().HasColumnName(nameof(DataType.DtTable).UnCapitalize())
            .HasMaxLength(100);
        builder.Property(e => e.DtIdFieldName).IsRequired().HasColumnName(nameof(DataType.DtIdFieldName).UnCapitalize())
            .HasMaxLength(50);
        builder.Property(e => e.DtKeyFieldName).HasColumnName(nameof(DataType.DtKeyFieldName).UnCapitalize())
            .HasMaxLength(50);
        builder.Property(e => e.DtNameFieldName).HasColumnName(nameof(DataType.DtNameFieldName).UnCapitalize())
            .HasMaxLength(50);
        builder.Property(e => e.DtGridRulesJson).HasColumnName(nameof(DataType.DtGridRulesJson).UnCapitalize())
            .HasColumnType(ConfigurationHelper.ColumnTypeNText);
        builder.HasOne(d => d.DtParentDataTypeNavigation).WithMany(p => p.ChildrenDataTypes)
            .HasForeignKey(d => d.DtParentDataTypeId).HasConstraintName(tableName.CreateSelfRelatedConstraintName(1));
    }
}