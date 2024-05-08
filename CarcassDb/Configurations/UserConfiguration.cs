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
        builder.HasIndex(e => e.NormalizedUserName)
            .HasDatabaseName(tableName.CreateIndexName(true, nameof(User.NormalizedUserName))).IsUnique();
        builder.HasIndex(e => e.NormalizedEmail)
            .HasDatabaseName(tableName.CreateIndexName(true, nameof(User.NormalizedEmail))).IsUnique();
        builder.Property(e => e.UsrId).HasColumnName(nameof(User.UsrId).UnCapitalize());
        builder.Property(e => e.UserName).IsRequired().HasColumnName(nameof(User.UserName).UnCapitalize())
            .HasMaxLength(256);
        builder.Property(e => e.NormalizedUserName).IsRequired()
            .HasColumnName(nameof(User.NormalizedUserName).UnCapitalize()).HasMaxLength(256);
        builder.Property(e => e.Email).IsRequired().HasColumnName(nameof(User.Email).UnCapitalize()).HasMaxLength(256);
        builder.Property(e => e.NormalizedEmail).IsRequired().HasColumnName(nameof(User.NormalizedEmail).UnCapitalize())
            .HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired().HasColumnName(nameof(User.PasswordHash).UnCapitalize())
            .HasMaxLength(256);
        builder.Property(e => e.FirstName).IsRequired().HasColumnName(nameof(User.FirstName).UnCapitalize())
            .HasMaxLength(50);
        builder.Property(e => e.LastName).IsRequired().HasColumnName(nameof(User.LastName).UnCapitalize())
            .HasMaxLength(100);
        builder.Property(e => e.FullName).IsRequired().HasColumnName(nameof(User.FullName).UnCapitalize())
            .HasMaxLength(100);
    }
}