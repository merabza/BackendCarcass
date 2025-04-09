using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public sealed class ReturnValueModelConfiguration : IEntityTypeConfiguration<ReturnValueModel>
{
    private const int KeyMaxLength = 512;
    private const int NameMaxLength = 512;

    public void Configure(EntityTypeBuilder<ReturnValueModel> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Key).HasMaxLength(KeyMaxLength);
        builder.Property(e => e.Name).HasMaxLength(NameMaxLength);
    }
}