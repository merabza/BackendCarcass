using System;
using System.Collections.Generic;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Identity.DependencyInjection;
using BackendCarcass.Application.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Application.Tests.TestInfrastructure;

// The real UserManager and RoleManager as AddCarcassIdentityCore registers them, over Carcass' own store
// (MyUserStore and IdentityRepository) and the carcass tables on in-memory SQLite
public sealed class IdentityManagersHarness : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public IdentityManagersHarness()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCarcassIdentityCore(configuration);
        services.AddSingleton<ICarcassApplicationDbContext>(Db.Context);
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddSingleton(Mock.Of<IDatabaseAbstraction>());
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
    }

    public CarcassSqliteDatabase Db { get; } = new();

    public UserManager<AppUser> UserManager => _scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    public RoleManager<AppRole> RoleManager => _scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    // SQLite-ის ტრიგერები ცხრილიდან წაშლას კრძალავს, რომ store-მა წაშლის შეცდომა დააბრუნოს
    public void ForbidUserDeletes()
    {
        Db.Context.Database.ExecuteSql(
            $"CREATE TRIGGER ForbidUsersDelete BEFORE DELETE ON Users BEGIN SELECT RAISE(ABORT, 'forbidden'); END");
    }

    public void ForbidRoleDeletes()
    {
        Db.Context.Database.ExecuteSql(
            $"CREATE TRIGGER ForbidRolesDelete BEFORE DELETE ON Roles BEGIN SELECT RAISE(ABORT, 'forbidden'); END");
    }

    public void ForbidRoleUpdates()
    {
        Db.Context.Database.ExecuteSql(
            $"CREATE TRIGGER ForbidRolesUpdate BEFORE UPDATE ON Roles BEGIN SELECT RAISE(ABORT, 'forbidden'); END");
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        Db.Dispose();
    }
}
