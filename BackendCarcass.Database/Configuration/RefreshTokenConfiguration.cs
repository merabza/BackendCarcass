using BackendCarcass.Domain.RefreshTokens;
using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemTools.DatabaseToolsShared;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Database.Configuration;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    private const int TokenHashMaxLength = 64;

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        string tableName = nameof(RefreshToken).Pluralize();
        builder.ToTable(tableName);

        builder.HasKey(e => e.RftId);
        builder.HasIndex(e => e.TokenHash).IsUnique();
        builder.HasIndex(e => e.UserId);

        builder.Property(e => e.TokenHash).IsRequired().HasMaxLength(TokenHashMaxLength);

        builder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName(tableName.CreateConstraintName(nameof(User)));
    }
}
