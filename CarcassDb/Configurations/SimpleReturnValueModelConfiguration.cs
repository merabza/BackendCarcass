using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarcassDb.Configurations;

public class SimpleReturnValueModelConfiguration : IEntityTypeConfiguration<SrvModel>
{
    private const int NameMaxLength = 512;

    public void Configure(EntityTypeBuilder<SrvModel> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(NameMaxLength);
    }
}