using BackendCarcass.Domain.EmailVerifications;
using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemTools.DatabaseToolsShared;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Database.Configuration;

public sealed class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    private const int HashMaxLength = 64;

    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        string tableName = nameof(EmailVerification).Pluralize();
        builder.ToTable(tableName);

        builder.HasKey(e => e.EvfId);
        builder.HasIndex(e => new { e.UserId, e.Purpose });

        builder.Property(e => e.CodeHash).IsRequired().HasMaxLength(HashMaxLength);
        builder.Property(e => e.LinkTokenHash).IsRequired().HasMaxLength(HashMaxLength);

        builder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName(tableName.CreateConstraintName(nameof(User)));
    }
}
