using System;
using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Identity.DependencyInjection;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Domain.DataTypes;
using BackendCarcass.Domain.ManyToManyJoins;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
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
    public const string UsersTable = "Users";
    public const string RolesTable = "Roles";

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
        //IdentityRepository.RolesByUsers User→Role წევრობას DataTypes-ის ცხრილების სახელებით არჩევს
        services.AddSingleton(Mock.Of<IDatabaseAbstraction>(d =>
            d.GetTableName<User>() == UsersTable && d.GetTableName<Role>() == RolesTable));
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
    }

    public CarcassSqliteDatabase Db { get; } = new();

    public UserManager<AppUser> UserManager => _scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    public RoleManager<AppRole> RoleManager => _scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    public Role AddRole(string key)
    {
        var role = new Role
        {
            RolKey = key, RolName = key + " როლი", RolLevel = 50, RolNormalizedKey = key.ToUpperInvariant()
        };
        Db.Context.Roles.Add(role);
        Db.Context.SaveChanges();
        return role;
    }

    // A User→Role row of ManyToManyJoins. The rights editor keys it by the DataTypes' keys
    // (PKey = NormalizedUserName, CKey = RolKey), the seed data (UsersByRoles.json) by UserName and RolKey
    public void AddMembership(string userKey, string roleKey)
    {
        Db.Context.ManyToManyJoins.Add(new ManyToManyJoin
        {
            PtId = DataTypeId(UsersTable), PKey = userKey, CtId = DataTypeId(RolesTable), CKey = roleKey
        });
        Db.Context.SaveChanges();
    }

    // The data type of the table, added on first use
    public int DataTypeId(string table)
    {
        DataType? dataType = Db.Context.DataTypes.SingleOrDefault(d => d.DtTable == table);
        if (dataType is null)
        {
            dataType = new DataType
            {
                DtName = table, DtNameNominative = table, DtNameGenitive = table, DtTable = table
            };
            Db.Context.DataTypes.Add(dataType);
            Db.Context.SaveChanges();
        }

        return dataType.DtId;
    }

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
