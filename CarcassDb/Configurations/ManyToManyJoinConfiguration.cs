using CarcassDb.Models;
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
        builder.HasIndex(e => new { e.PtId, e.PKey, e.CtId, e.CKey }).HasDatabaseName(tableName.CreateIndexName(true,
            nameof(ManyToManyJoin.PtId), nameof(ManyToManyJoin.PKey), nameof(ManyToManyJoin.CtId),
            nameof(ManyToManyJoin.CKey))).IsUnique();
        builder.Property(e => e.MmjId).HasColumnName(nameof(ManyToManyJoin.MmjId).UnCapitalize());
        builder.Property(e => e.CKey).HasColumnName(nameof(ManyToManyJoin.CKey).UnCapitalize()).HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.CtId).HasColumnName(nameof(ManyToManyJoin.CtId).UnCapitalize());
        builder.Property(e => e.PKey).HasColumnName(nameof(ManyToManyJoin.PKey).UnCapitalize()).HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.PtId).HasColumnName(nameof(ManyToManyJoin.PtId).UnCapitalize());
        builder.HasOne(d => d.ParentDataTypeNavigation).WithMany(p => p.ManyToManyJoinParentTypes)
            .HasForeignKey(d => d.PtId).OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName($"{dataTypeConstraintName}_Parent");
        builder.HasOne(d => d.ChildDataTypeNavigation).WithMany(p => p.ManyToManyJoinChildTypes)
            .HasForeignKey(d => d.CtId).OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName($"{dataTypeConstraintName}_Child");
    }
}