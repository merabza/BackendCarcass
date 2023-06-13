using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class AppClaimConfiguration : IEntityTypeConfiguration<AppClaim>
{
    public void Configure(EntityTypeBuilder<AppClaim> builder)
    {
        var tableName = nameof(AppClaim).Pluralize();

        builder.HasKey(e => e.AclId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.AclKey).HasDatabaseName(tableName.CreateIndexName(true, nameof(AppClaim.AclKey)))
            .IsUnique();
        builder.Property(e => e.AclId).HasColumnName(nameof(AppClaim.AclId).UnCapitalize());
        builder.Property(e => e.AclKey).HasColumnName(nameof(AppClaim.AclKey).UnCapitalize()).HasMaxLength(50)
            .IsRequired();
        builder.Property(e => e.AclName).HasColumnName(nameof(AppClaim.AclName).UnCapitalize()).HasMaxLength(50)
            .IsRequired();
    }
}