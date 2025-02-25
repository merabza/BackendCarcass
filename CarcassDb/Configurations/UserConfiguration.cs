using CarcassDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemToolsShared;

namespace CarcassDb.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        var tableName = nameof(User).Pluralize();

        builder.HasKey(e => e.UsrId);
        builder.ToTable(tableName.UnCapitalize());
        builder.HasIndex(e => e.NormalizedUserName).IsUnique();
        builder.HasIndex(e => e.NormalizedEmail).IsUnique();
        builder.Property(e => e.UserName).IsRequired().HasMaxLength(256);
        builder.Property(e => e.NormalizedUserName).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.NormalizedEmail).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(100);
    }
}