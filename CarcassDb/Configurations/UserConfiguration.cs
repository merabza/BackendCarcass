using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const int UserNameMaxLength = 256;
    public const int NormalizedUserNameMaxLength = 256;
    public const int EmailMaxLength = 256;
    public const int NormalizedEmailMaxLength = 256;
    public const int PasswordHashMaxLength = 256;
    public const int FirstNameMaxLength = 50;
    public const int LastNameMaxLength = 100;
    public const int FullNameMaxLength = 100;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        //var tableName = nameof(User).Pluralize();
        //builder.ToTable(tableName.UnCapitalize());

        builder.HasKey(e => e.UsrId);
        builder.HasIndex(e => e.NormalizedUserName).IsUnique();
        builder.HasIndex(e => e.NormalizedEmail).IsUnique();

        builder.Property(e => e.UserName).IsRequired().HasMaxLength(UserNameMaxLength);
        builder.Property(e => e.NormalizedUserName).IsRequired().HasMaxLength(NormalizedUserNameMaxLength);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(EmailMaxLength);
        builder.Property(e => e.NormalizedEmail).IsRequired().HasMaxLength(NormalizedEmailMaxLength);
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(PasswordHashMaxLength);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(FirstNameMaxLength);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(LastNameMaxLength);
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(FullNameMaxLength);
    }
}