using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class ReturnValueModelConfiguration : IEntityTypeConfiguration<ReturnValueModel>
{
    public const int KeyMaxLength = 512;
    public const int NameMaxLength = 512;

    public void Configure(EntityTypeBuilder<ReturnValueModel> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Key).HasMaxLength(KeyMaxLength);
        builder.Property(e => e.Name).HasMaxLength(NameMaxLength);
    }
}