using CarcassMasterDataDom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class ReturnValueModelConfiguration : IEntityTypeConfiguration<ReturnValueModel>
{
    public void Configure(EntityTypeBuilder<ReturnValueModel> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);
        builder.Property(e => e.Id).HasColumnName(nameof(ReturnValueModel.Id).UnCapitalize()).IsRequired();
        builder.Property(e => e.Key).HasColumnName(nameof(ReturnValueModel.Key).UnCapitalize()).HasMaxLength(512);
        builder.Property(e => e.Name).HasColumnName(nameof(ReturnValueModel.Name).UnCapitalize()).HasMaxLength(512);
        builder.Property(e => e.ParentId).HasColumnName(nameof(ReturnValueModel.ParentId).UnCapitalize());
    }
}