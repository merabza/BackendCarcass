using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class ReturnValueModelConfiguration : IEntityTypeConfiguration<ReturnValueModel>
{
    public void Configure(EntityTypeBuilder<ReturnValueModel> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);
        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Key).HasMaxLength(512);
        builder.Property(e => e.Name).HasMaxLength(512);
    }
}