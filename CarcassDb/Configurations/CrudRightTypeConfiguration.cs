using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public sealed class CrudRightTypeConfiguration : IEntityTypeConfiguration<CrudRightType>
{
    private const int CrtKeyMaxLength = 50;
    private const int CrtNameMaxLength = 50;

    public void Configure(EntityTypeBuilder<CrudRightType> builder)
    {
        builder.HasKey(e => e.CrtId);
        builder.HasIndex(e => e.CrtKey).IsUnique();

        builder.Property(e => e.CrtKey).IsRequired().HasMaxLength(CrtKeyMaxLength);
        builder.Property(e => e.CrtName).IsRequired().HasMaxLength(CrtNameMaxLength);
    }
}