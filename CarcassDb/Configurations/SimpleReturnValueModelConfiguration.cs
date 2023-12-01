using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class SimpleReturnValueModelConfiguration : IEntityTypeConfiguration<SrvModel>
{
    public void Configure(EntityTypeBuilder<SrvModel> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);
        builder.Property(e => e.Id).HasColumnName(nameof(SrvModel.Id).UnCapitalize()).IsRequired();
        builder.Property(e => e.Name).HasColumnName(nameof(SrvModel.Name).UnCapitalize()).HasMaxLength(512);
    }
}