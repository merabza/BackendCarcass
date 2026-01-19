using System;
using System.Text;
using CarcassIdentity.Models;
using CarcassMasterData.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace CarcassIdentity.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassIdentityDependencyInjection
{
    public static IServiceCollection AddCarcassIdentity(this IServiceCollection services, ILogger logger,
        IConfiguration configuration, bool debugMode)
    {
        if (debugMode)
        {
            logger.Information("{MethodName} Started", nameof(AddCarcassIdentity));
        }

        services.AddScoped<IUserStore<AppUser>, MyUserStore>();
        services.AddScoped<IUserPasswordStore<AppUser>, MyUserStore>();
        services.AddScoped<IUserEmailStore<AppUser>, MyUserStore>();
        services.AddScoped<IUserRoleStore<AppUser>, MyUserStore>();
        services.AddScoped<IRoleStore<AppRole>, MyUserStore>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.Password.RequiredLength = 3;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
        }).AddDefaultTokenProviders();

        // configure strongly typed settings objects
        IConfigurationSection appSettingsSection = configuration.GetSection("IdentitySettings");
        services.Configure<IdentitySettings>(appSettingsSection);

        // configure jwt authentication
        IdentitySettings identitySettings = appSettingsSection.Get<IdentitySettings>() ??
                                            throw new Exception("IdentitySettings is null");
        string jwtSecret = identitySettings.JwtSecret ?? throw new Exception("jwtSecret is null");
        byte[] key = Encoding.ASCII.GetBytes(jwtSecret);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(key)
                //ValidateIssuer = false,
                //ValidateAudience = false
            };
        });

        services.AddAuthorizationBuilder().SetInvokeHandlersAfterFailure(true);

        if (debugMode)
        {
            logger.Information("{MethodName} Finished", nameof(AddCarcassIdentity));
        }

        return services;
    }

    public static bool UseAuthenticationAndAuthorization(this IApplicationBuilder app, ILogger logger, bool debugMode)
    {
        if (debugMode)
        {
            logger.Information("{MethodName} Started", nameof(UseAuthenticationAndAuthorization));
        }

        app.UseAuthentication();
        app.UseAuthorization();

        if (debugMode)
        {
            logger.Information("{MethodName} Finished", nameof(UseAuthenticationAndAuthorization));
        }

        return true;
    }
}
