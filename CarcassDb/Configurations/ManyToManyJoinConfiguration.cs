using CarcassDb.Models;
using DatabaseToolsShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class ManyToManyJoinConfiguration : IEntityTypeConfiguration<ManyToManyJoin>
{
    public void Configure(EntityTypeBuilder<ManyToManyJoin> builder)
    {
        var tableName = nameof(ManyToManyJoin).Pluralize();
        var dataTypeConstraintName = tableName.CreateConstraintName(nameof(DataType));

        builder.HasKey(e => e.MmjId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => new { e.PtId, e.PKey, e.CtId, e.CKey }).IsUnique();
        builder.Property(e => e.CKey).HasMaxLength(100).IsRequired();
        builder.Property(e => e.PKey).HasMaxLength(100).IsRequired();
        builder.HasOne(d => d.ParentDataTypeNavigation).WithMany(p => p.ManyToManyJoinParentTypes)
            .HasForeignKey(d => d.PtId).OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName($"{dataTypeConstraintName}_Parent");
        builder.HasOne(d => d.ChildDataTypeNavigation).WithMany(p => p.ManyToManyJoinChildTypes)
            .HasForeignKey(d => d.CtId).OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName($"{dataTypeConstraintName}_Child");
    }
}