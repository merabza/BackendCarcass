using System;
using BackendCarcass.Database;
using BackendCarcass.Domain.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Tests.TestInfrastructure;

// The real EF model of the carcass tables over an in-memory SQLite database, so the services that query
// EmailVerifications and RefreshTokens run against actual SQL (ordering, ExecuteDelete, foreign keys) instead of
// a mocked DbSet
public sealed class CarcassSqliteDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public CarcassSqliteDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        DbContextOptions<CarcassDbContext> options =
            new DbContextOptionsBuilder<CarcassDbContext>().UseSqlite(_connection).Options;
        Context = new CarcassDbContext(options, new Mock<IDomainEventsDispatcher>().Object);
        Context.Database.EnsureCreated();
    }

    public CarcassDbContext Context { get; }

    public User AddUser(string userName = "tester", string email = "tester@example.com")
    {
        var user = new User
        {
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = "hash",
            FullName = "Test User",
            FirstName = "Test",
            LastName = "User"
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        return user;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
