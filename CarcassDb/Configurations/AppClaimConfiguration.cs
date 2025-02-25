using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class AppClaimConfiguration : IEntityTypeConfiguration<AppClaim>
{
    public void Configure(EntityTypeBuilder<AppClaim> builder)
    {
        builder.HasKey(e => e.AclId);
        builder.HasIndex(e => e.AclKey).IsUnique();
        builder.Property(e => e.AclKey).HasMaxLength(50).IsRequired();
        builder.Property(e => e.AclName).HasMaxLength(50).IsRequired();
    }
}