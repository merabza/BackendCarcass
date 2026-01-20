using BackendCarcass.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCarcass.DatabaseConfiguration;

public sealed class AppClaimConfiguration : IEntityTypeConfiguration<AppClaim>
{
    private const int AclKeyMaxLength = 50;
    private const int AclNameMaxLength = 50;

    public void Configure(EntityTypeBuilder<AppClaim> builder)
    {
        //var tableName = nameof(AppClaim).Pluralize();
        //builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.AclId);
        builder.HasIndex(e => e.AclKey).IsUnique();
        builder.Property(e => e.AclKey).HasMaxLength(AclKeyMaxLength).IsRequired();
        builder.Property(e => e.AclName).HasMaxLength(AclNameMaxLength).IsRequired();
    }
}