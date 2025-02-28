﻿using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class AppClaimConfiguration : IEntityTypeConfiguration<AppClaim>
{
    public const int AclKeyMaxLength = 50;
    public const int AclNameMaxLength = 50;

    public void Configure(EntityTypeBuilder<AppClaim> builder)
    {
        builder.HasKey(e => e.AclId);
        builder.HasIndex(e => e.AclKey).IsUnique();
        builder.Property(e => e.AclKey).HasMaxLength(AclKeyMaxLength).IsRequired();
        builder.Property(e => e.AclName).HasMaxLength(AclNameMaxLength).IsRequired();
    }
}