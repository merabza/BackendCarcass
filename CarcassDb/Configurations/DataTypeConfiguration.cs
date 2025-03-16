using CarcassDb.Models;
using DatabaseToolsShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class DataTypeConfiguration : IEntityTypeConfiguration<DataType>
{
    public const int DtKeyMaxLength = 36;
    public const int DtNameMaxLength = 100;
    public const int DtNameNominativeMaxLength = 100;
    public const int DtNameGenitiveMaxLength = 100;
    public const int DtTableMaxLength = 100;
    public const int DtIdFieldNameMaxLength = 50;
    public const int DtKeyFieldNameMaxLength = 50;
    public const int DtNameFieldNameMaxLength = 50;

    public void Configure(EntityTypeBuilder<DataType> builder)
    {
        //var tableName = nameof(DataType).Pluralize();
        //builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.DtId);
        builder.HasIndex(e => e.DtKey).IsUnique();
        builder.HasIndex(e => e.DtTable).IsUnique();

        builder.Property(e => e.DtKey).IsRequired().HasMaxLength(DtKeyMaxLength);
        builder.Property(e => e.DtName).IsRequired().HasMaxLength(DtNameMaxLength);
        builder.Property(e => e.DtNameNominative).IsRequired().HasMaxLength(DtNameNominativeMaxLength);
        builder.Property(e => e.DtNameGenitive).IsRequired().HasMaxLength(DtNameGenitiveMaxLength);
        builder.Property(e => e.DtTable).IsRequired().HasMaxLength(DtTableMaxLength);
        builder.Property(e => e.DtIdFieldName).HasMaxLength(DtIdFieldNameMaxLength);
        builder.Property(e => e.DtKeyFieldName).HasMaxLength(DtKeyFieldNameMaxLength);
        builder.Property(e => e.DtNameFieldName).HasMaxLength(DtNameFieldNameMaxLength);
        builder.Property(e => e.DtGridRulesJson).HasColumnType(ConfigurationHelper.ColumnTypeNText);

        builder.HasOne(d => d.DtParentDataTypeNavigation).WithMany(p => p.ChildrenDataTypes)
            .HasForeignKey(d => d.DtParentDataTypeId);

        builder.HasOne(d => d.DtManyToManyJoinParentDataTypeNavigation).WithMany(p => p.ManyJoinParentDataTypes)
            .HasForeignKey(d => d.DtManyToManyJoinParentDataTypeId);

        builder.HasOne(d => d.DtManyToManyJoinChildDataTypeNavigation).WithMany(p => p.ManyToManyJoinChildrenDataTypes)
            .HasForeignKey(d => d.DtManyToManyJoinChildDataTypeId);
    }
}