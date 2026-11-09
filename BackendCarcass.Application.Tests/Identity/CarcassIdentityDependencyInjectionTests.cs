using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Application.Email;
using BackendCarcass.Application.Identity.DependencyInjection;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BackendCarcass.Application.Tests.Identity;

// AddCarcassIdentity still configures everything the apps had (Identity, JWT bearer as the default scheme, e-mail),
// while its parts can be called alone: the core gives Identity and the default sign-in implementations without JWT,
// and a host's own implementations registered first are kept
public sealed class CarcassIdentityDependencyInjectionTests
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["IdentitySettings:JwtSecret"] = "0123456789abcdef0123456789abcdef0123456789abcdef",
            ["IdentitySettings:JwtIssuer"] = "https://issuer.test",
            ["IdentitySettings:JwtAudience"] = "https://audience.test",
            ["IdentitySettings:AllowEmailSignIn"] = "true",
            ["IdentitySettings:ConfirmEmailPath"] = "/account/confirm-email"
        }).Build();

    [Fact]
    public void The_full_registration_keeps_jwt_bearer_as_the_default_scheme_and_the_defaults()
    {
        var services = new ServiceCollection();

        services.AddCarcassIdentity(null, Configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        AuthenticationOptions options = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, options.DefaultScheme);
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, options.DefaultChallengeScheme);
        Assert.Contains(options.Schemes, s => s.Name == JwtBearerDefaults.AuthenticationScheme);
        Assert.Equal(typeof(JwtLoginTokenIssuer), ImplementationOf<ILoginTokenIssuer>(services));
        Assert.Equal(typeof(JwtPendingLoginStore), ImplementationOf<IPendingLoginStore>(services));
        Assert.Contains(services, d => d.ServiceType == typeof(IEmailSender));
    }

    [Fact]
    public void The_core_alone_has_no_jwt_bearer_and_reads_the_settings()
    {
        var services = new ServiceCollection();

        services.AddCarcassIdentityCore(Configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        AuthenticationOptions options = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        Assert.DoesNotContain(options.Schemes, s => s.Name == JwtBearerDefaults.AuthenticationScheme);
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IEmailSender));
        IdentitySettings settings = provider.GetRequiredService<IOptions<IdentitySettings>>().Value;
        Assert.True(settings.AllowEmailSignIn);
        Assert.Equal("/account/confirm-email", settings.ConfirmEmailPath);
        Assert.Equal("/reset-password", settings.ResetPasswordPath);
    }

    [Fact]
    public void Implementations_registered_by_the_host_first_are_kept()
    {
        var services = new ServiceCollection();
        ILoginTokenIssuer issuer = Mock.Of<ILoginTokenIssuer>();
        IPendingLoginStore store = Mock.Of<IPendingLoginStore>();
        services.AddScoped(_ => issuer);
        services.AddScoped(_ => store);

        services.AddCarcassIdentityCore(Configuration);

        Assert.Single(services, d => d.ServiceType == typeof(ILoginTokenIssuer));
        Assert.Single(services, d => d.ServiceType == typeof(IPendingLoginStore));
        Assert.Null(ImplementationOf<ILoginTokenIssuer>(services));
    }

    [Fact]
    public void The_defaults_of_the_new_settings_keep_the_old_behaviour()
    {
        var settings = new IdentitySettings();

        Assert.False(settings.AllowEmailSignIn);
        Assert.Equal("/confirm-email", settings.ConfirmEmailPath);
        Assert.Equal("/reset-password", settings.ResetPasswordPath);
    }

    private static System.Type? ImplementationOf<TService>(ServiceCollection services)
    {
        return services.Single(d => d.ServiceType == typeof(TService)).ImplementationType;
    }
}
