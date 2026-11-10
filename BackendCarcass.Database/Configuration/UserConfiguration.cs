using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendCarcass.Database.Configuration;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const int UserNameMaxLength = 256;
    private const int NormalizedUserNameMaxLength = 256;
    private const int EmailMaxLength = 256;
    private const int NormalizedEmailMaxLength = 256;
    private const int PasswordHashMaxLength = 256;
    private const int FirstNameMaxLength = 50;
    private const int LastNameMaxLength = 100;
    private const int FullNameMaxLength = 100;
    private const int SsoSubjectMaxLength = 64;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.UsrId);
        builder.HasIndex(e => e.NormalizedUserName).IsUnique();
        builder.HasIndex(e => e.NormalizedEmail).IsUnique();
        //უნიკალური მხოლოდ შევსებულ მნიშვნელობებს შორის: SQL Server-ზე EF ფილტრს (IS NOT NULL) თვითონ ამატებს
        builder.HasIndex(e => e.SsoSubject).IsUnique().HasDatabaseName("IX_Users_SsoSubject");

        builder.Property(e => e.UserName).IsRequired().HasMaxLength(UserNameMaxLength);
        builder.Property(e => e.NormalizedUserName).IsRequired().HasMaxLength(NormalizedUserNameMaxLength);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(EmailMaxLength);
        builder.Property(e => e.NormalizedEmail).IsRequired().HasMaxLength(NormalizedEmailMaxLength);
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(PasswordHashMaxLength);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(FirstNameMaxLength);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(LastNameMaxLength);
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(FullNameMaxLength);
        builder.Property(e => e.EmailConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.AccessFailedCount).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.SsoSubject).HasMaxLength(SsoSubjectMaxLength);
    }
}
