using System;
using System.Collections.Generic;
using System.Text;
using CarcassIdentity.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WebInstallers;

namespace CarcassIdentity.Installers;

// ReSharper disable once UnusedType.Global
public sealed class CarcassIdentityInstaller : IInstaller
{
    public int InstallPriority => 27;
    public int ServiceUsePriority => 60;

    public bool InstallServices(WebApplicationBuilder builder, bool debugMode, string[] args,
        Dictionary<string, string> parameters)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(InstallServices)} Started");

        builder.Services.AddScoped<IUserStore<AppUser>, MyUserStore>();
        builder.Services.AddScoped<IUserPasswordStore<AppUser>, MyUserStore>();
        builder.Services.AddScoped<IUserEmailStore<AppUser>, MyUserStore>();
        builder.Services.AddScoped<IUserRoleStore<AppUser>, MyUserStore>();
        builder.Services.AddScoped<IRoleStore<AppRole>, MyUserStore>();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();


        builder.Services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.Password.RequiredLength = 3;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
        }).AddDefaultTokenProviders();

        // configure strongly typed settings objects
        var appSettingsSection = builder.Configuration.GetSection("IdentitySettings");
        builder.Services.Configure<IdentitySettings>(appSettingsSection);

        // configure jwt authentication
        var identitySettings = appSettingsSection.Get<IdentitySettings>() ??
                               throw new Exception("IdentitySettings is null");
        var jwtSecret = identitySettings.JwtSecret ?? throw new Exception("jwtSecret is null");
        var key = Encoding.ASCII.GetBytes(jwtSecret);

        builder.Services.AddAuthentication(x =>
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
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        builder.Services.AddAuthorization(x => { x.InvokeHandlersAfterFailure = true; });

        if (debugMode)
            Console.WriteLine("CarcassIdentityInstaller.InstallServices Finished");

        return true;
    }

    public bool UseServices(WebApplication app, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Started");

        app.UseAuthentication();
        app.UseAuthorization();

        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Finished");

        return true;
    }
}