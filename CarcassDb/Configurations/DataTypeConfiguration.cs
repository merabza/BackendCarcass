using CarcassDb.Models;
using DatabaseToolsShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class DataTypeConfiguration : IEntityTypeConfiguration<DataType>
{
    public void Configure(EntityTypeBuilder<DataType> builder)
    {
        var tableName = nameof(DataType).Pluralize();
        builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.DtId);
        builder.HasIndex(e => e.DtKey).IsUnique();
        builder.HasIndex(e => e.DtTable).IsUnique();
        builder.Property(e => e.DtKey).IsRequired().HasMaxLength(36);
        builder.Property(e => e.DtName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DtNameNominative).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DtNameGenitive).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DtTable).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DtIdFieldName).HasMaxLength(50);
        builder.Property(e => e.DtKeyFieldName).HasMaxLength(50);
        builder.Property(e => e.DtNameFieldName).HasMaxLength(50);
        builder.Property(e => e.DtGridRulesJson).HasColumnType(ConfigurationHelper.ColumnTypeNText);

        builder.HasOne(d => d.DtParentDataTypeNavigation).WithMany(p => p.ChildrenDataTypes)
            .HasForeignKey(d => d.DtParentDataTypeId);

        builder.HasOne(d => d.DtManyToManyJoinParentDataTypeNavigation).WithMany(p => p.ManyJoinParentDataTypes)
            .HasForeignKey(d => d.DtManyToManyJoinParentDataTypeId);

        builder.HasOne(d => d.DtManyToManyJoinChildDataTypeNavigation).WithMany(p => p.ManyToManyJoinChildrenDataTypes)
            .HasForeignKey(d => d.DtManyToManyJoinChildDataTypeId);
    }
}