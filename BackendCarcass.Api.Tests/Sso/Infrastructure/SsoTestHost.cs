using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BackendCarcass.Api.DependencyInjection;
using BackendCarcass.Application;
using BackendCarcass.Application.DependencyInjection;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Identity.DependencyInjection;
using BackendCarcass.Application.Identity.Sso;
using BackendCarcass.Application.Repositories.DependencyInjection;
using BackendCarcass.Database;
using BackendCarcass.Domain.DataTypes;
using BackendCarcass.Domain.ManyToManyJoins;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SystemTools.Application.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Api.Tests.Sso.Infrastructure;

//Carcass-ზე აგებული აპის ჰოსტი TestServer-ზე: ნამდვილი BackendCarcass-ის რეგისტრაციები და endpoint-ები,
//SQLite მეხსიერებაში, sso-ს ნაცვლად FakeSsoServer, დრო — ManualTimeProvider
internal sealed class SsoTestHost : IAsyncDisposable
{
    public const string CookieName = "errata.bff";
    public const string MachineClientId = "grammar-worker";
    public const string MachineUserName = "worker";
    public const string AdminRole = "Admin";
    public const string MeRoute = "/api/v1/test/me";
    public const string HubRoute = "/api/v1/test/hub";

    private readonly SqliteConnection _connection;

    private SsoTestHost(WebApplication app, SqliteConnection connection, FakeSsoServer sso, ManualTimeProvider time)
    {
        App = app;
        _connection = connection;
        Sso = sso;
        Time = time;
    }

    public WebApplication App { get; }
    public FakeSsoServer Sso { get; }
    public ManualTimeProvider Time { get; }

    public static async Task<SsoTestHost> StartAsync(EAuthenticationMode mode = EAuthenticationMode.Sso)
    {
        var sso = new FakeSsoServer();
        var time = new ManualTimeProvider(DateTimeOffset.UtcNow);
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        DbContextOptions<CarcassDbContext> dbOptions =
            new DbContextOptionsBuilder<CarcassDbContext>().UseSqlite(connection).Options;

        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseTestServer();
        //აპის სერვისები (IUnitOfWork, IMasterDataLoaderCreator) აქ არ არის; ტესტები მათ ჰენდლერებს არ იძახებს
        builder.Host.UseDefaultServiceProvider(o =>
        {
            o.ValidateOnBuild = false;
            o.ValidateScopes = true;
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:Mode"] = mode.ToString(),
            ["Authentication:CookieName"] = CookieName,
            ["Authentication:Sso:Authority"] = FakeSsoServer.Issuer,
            ["Authentication:Sso:ClientId"] = FakeSsoServer.ClientId,
            ["Authentication:Sso:ClientSecret"] = FakeSsoServer.ClientSecret,
            ["Authentication:Sso:ApiScope"] = FakeSsoServer.ApiAudience,
            ["Authentication:MachineClients:" + MachineClientId] = MachineUserName,
            ["IdentitySettings:JwtSecret"] = "made-up-local-mode-secret-made-up-local-mode-secret",
            ["IdentitySettings:JwtIssuer"] = "test",
            ["IdentitySettings:JwtAudience"] = "test"
        });

        IServiceCollection services = builder.Services;
        services.AddSingleton<TimeProvider>(time);
        services.AddScoped<ICarcassApplicationDbContext>(_ =>
            new CarcassDbContext(dbOptions, new Mock<IDomainEventsDispatcher>().Object));
        services.AddScoped<IDatabaseAbstraction, TestDatabaseAbstraction>();
        services.AddCors(o => o.AddPolicy("cors", p => p.AllowAnyOrigin()));
        services.AddSignalR();
        services.AddApplication(null, typeof(Application.AssemblyReference));
        services.AddCarcassRepositories(null);
        services.AddCarcassIdentity(null, builder.Configuration);
        services.AddScopedAllCarcassApplicationServices(null);
        //sso-ს discovery და HTTP — FakeSsoServer (Configure PostConfigure-მდე სრულდება)
        services.Configure<OpenIdConnectOptions>(CarcassSsoDefaults.OpenIdConnectScheme, o =>
        {
            o.Configuration = sso.Configuration;
            o.Backchannel = new HttpClient(sso, false);
        });
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
            o => o.Configuration = sso.Configuration);

        WebApplication app = builder.Build();
        await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            var db = (CarcassDbContext)scope.ServiceProvider.GetRequiredService<ICarcassApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();
            await SeedAsync(db);
        }

        app.UseCors();
        app.UseAuthenticationAndAuthorization(null);
        app.UseBackendCarcassApiEndpoints("cors", null);
        app.MapGet(MeRoute, (ICurrentUser user) => $"{user.Id}|{user.Name}|{string.Join(",", user.Roles)}")
            .RequireAuthorization();
        app.MapHub<TestHub>(HubRoute).RequireAuthorization();
        await app.StartAsync();

        return new SsoTestHost(app, connection, sso, time);
    }

    public SsoBrowser NewBrowser()
    {
        return new SsoBrowser(this);
    }

    public HttpMessageHandler CreateHandler()
    {
        return App.GetTestServer().CreateHandler();
    }

    public IEnumerable<string> RoutePatterns()
    {
        return ((IEndpointRouteBuilder)App).DataSources.SelectMany(d => d.Endpoints).OfType<RouteEndpoint>()
            .Select(e => "/" + e.RoutePattern.RawText!.TrimStart('/'));
    }

    public async Task<T> WithDbAsync<T>(Func<CarcassDbContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        return await action((CarcassDbContext)scope.ServiceProvider.GetRequiredService<ICarcassApplicationDbContext>());
    }

    public Task<User> AddLocalUserAsync(string userName, string email, string? ssoSubject = null,
        params string[] roles)
    {
        return WithDbAsync(async db =>
        {
            var user = new User
            {
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = "made-up-hash",
                FullName = "Local User",
                FirstName = "Local",
                LastName = "User",
                SsoSubject = ssoSubject
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            foreach (string role in roles)
            {
                await AddRoleToUserAsync(db, userName, role);
            }

            return user;
        });
    }

    public Task<int> AddRoleToUserAsync(string userName, string role)
    {
        return WithDbAsync(db => AddRoleToUserAsync(db, userName, role));
    }

    public async ValueTask DisposeAsync()
    {
        await App.DisposeAsync();
        await _connection.DisposeAsync();
        Sso.Dispose();
    }

    private static async Task<int> AddRoleToUserAsync(CarcassDbContext db, string userName, string role)
    {
        int usersType = await DataTypeId(db, db.GetTableName<User>());
        int rolesType = await DataTypeId(db, db.GetTableName<Role>());
        db.ManyToManyJoins.Add(new ManyToManyJoin { PtId = usersType, PKey = userName, CtId = rolesType, CKey = role });
        return await db.SaveChangesAsync();
    }

    private static Task<int> DataTypeId(CarcassDbContext db, string table)
    {
        return db.DataTypes.Where(d => d.DtTable == table).Select(d => d.DtId).SingleAsync();
    }

    private static async Task SeedAsync(CarcassDbContext db)
    {
        foreach (string table in new[] { db.GetTableName<User>(), db.GetTableName<Role>() })
        {
            db.DataTypes.Add(new DataType
            {
                DtName = table, DtNameNominative = table, DtNameGenitive = table, DtTable = table
            });
        }

        db.Roles.Add(new Role { RolKey = AdminRole, RolName = AdminRole, RolNormalizedKey = AdminRole.ToUpperInvariant() });
        await db.SaveChangesAsync();

        db.Users.Add(new User
        {
            UserName = MachineUserName,
            NormalizedUserName = MachineUserName.ToUpperInvariant(),
            Email = "worker@example.com",
            NormalizedEmail = "WORKER@EXAMPLE.COM",
            PasswordHash = "made-up-hash",
            FullName = "Worker",
            FirstName = "Worker",
            LastName = "Worker"
        });
        await db.SaveChangesAsync();
        await AddRoleToUserAsync(db, MachineUserName, AdminRole);
    }

    private sealed class TestHub : Hub;

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class TestDatabaseAbstraction(ICarcassApplicationDbContext db) : IDatabaseAbstraction
    {
        public string GetTableName<T>() where T : class
        {
            return db.GetTableName<T>();
        }

        public Task<Result> ExecuteSqlRawRetOptionAsync(string sql,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success());
        }

        public void SetCommandTimeout(TimeSpan timeout)
        {
        }
    }
}
