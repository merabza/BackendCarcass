﻿using CarcassDb.Models;
using DatabaseToolsShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class ManyToManyJoinConfiguration : IEntityTypeConfiguration<ManyToManyJoin>
{
    private const int CKeyMaxLength = 100;
    private const int PKeyMaxLength = 100;

    public void Configure(EntityTypeBuilder<ManyToManyJoin> builder)
    {
        var tableName = nameof(ManyToManyJoin).Pluralize();
        builder.ToTable(tableName);

        var dataTypeConstraintName = tableName.CreateConstraintName(nameof(DataType));

        builder.HasKey(e => e.MmjId);
        builder.HasIndex(e => new { e.PtId, e.PKey, e.CtId, e.CKey }).IsUnique();

        builder.Property(e => e.CKey).HasMaxLength(CKeyMaxLength).IsRequired();
        builder.Property(e => e.PKey).HasMaxLength(PKeyMaxLength).IsRequired();

        builder.HasOne(d => d.ParentDataTypeNavigation).WithMany(p => p.ManyToManyJoinParentTypes)
            .HasForeignKey(d => d.PtId).OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName($"{dataTypeConstraintName}_Parent");

        builder.HasOne(d => d.ChildDataTypeNavigation).WithMany(p => p.ManyToManyJoinChildTypes)
            .HasForeignKey(d => d.CtId).OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName($"{dataTypeConstraintName}_Child");
    }
}