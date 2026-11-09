using System;
using System.Text;
using System.Threading.RateLimiting;
using BackendCarcass.Application.Email;
using BackendCarcass.Application.Identity.Models;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace BackendCarcass.Application.Identity.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class CarcassIdentityDependencyInjection
{
    //ავტორიზაციის endpoint-ების მოთხოვნების სიხშირის შეზღუდვის პოლისის სახელი
    public const string AuthenticationRateLimitPolicy = "CarcassAuthentication";

    private const string IdentitySettingsSectionName = "IdentitySettings";

    //პაროლის წესები. დანარჩენი წესები (ასო, მომხმარებლის სახელი, გავრცელებული პაროლები) CarcassPasswordValidator-შია
    private const int PasswordRequiredLength = 12;
    private const int PasswordRequiredUniqueChars = 5;

    //ანგარიშის დროებითი ბლოკირება წარუმატებელი ავტორიზაციის ცდების შემდეგ
    private const int MaxFailedAccessAttempts = 5;
    private const int LockoutMinutes = 15;

    //ერთი IP მისამართიდან ავტორიზაციის მოთხოვნების მაქსიმალური რაოდენობა წუთში
    private const int AuthenticationRequestsPerMinutePerIp = 30;

    //ავტორიზაციის სრული კონფიგურაცია: Identity, აპის JWT, ელფოსტა და მოთხოვნების სიხშირის შეზღუდვა.
    //ჰოსტს, რომელსაც JWT არ სჭირდება (მაგალითად sso.ltg.ge), ცალკე ნაწილების გამოძახება შეუძლია
    public static IServiceCollection AddCarcassIdentity(this IServiceCollection services, ILogger? debugLogger,
        IConfiguration configuration)
    {
        debugLogger?.Information("{MethodName} Started", nameof(AddCarcassIdentity));

        services.AddCarcassIdentityCore(configuration);
        services.AddCarcassJwtAuthentication(configuration);

        services.AddAuthorizationBuilder().SetInvokeHandlersAfterFailure(true);

        services.AddCarcassEmailSending(configuration);
        services.AddCarcassAuthenticationRateLimiter();

        debugLogger?.Information("{MethodName} Finished", nameof(AddCarcassIdentity));

        return services;
    }

    //Identity საკუთარი store-ით, პაროლის წესები, ანგარიშის ბლოკირება, IdentitySettings და შესვლის ნაგულისხმევი
    //რეალიზაციები (ILoginTokenIssuer, IPendingLoginStore). ისინი TryAdd-ით რეგისტრირდება, ამიტომ ჰოსტის მიერ
    //ადრე დარეგისტრირებული რეალიზაცია რჩება
    public static IServiceCollection AddCarcassIdentityCore(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUserStore<AppUser>, MyUserStore>();
        services.AddScoped<IUserPasswordStore<AppUser>, MyUserStore>();
        services.AddScoped<IUserEmailStore<AppUser>, MyUserStore>();
        services.AddScoped<IUserRoleStore<AppUser>, MyUserStore>();
        services.AddScoped<IUserLockoutStore<AppUser>, MyUserStore>();
        services.AddScoped<IRoleStore<AppRole>, MyUserStore>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = PasswordRequiredLength;
                options.Password.RequiredUniqueChars = PasswordRequiredUniqueChars;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(LockoutMinutes);
            }).AddDefaultTokenProviders().AddErrorDescriber<GeorgianIdentityErrorDescriber>()
            .AddPasswordValidator<CarcassPasswordValidator>();

        // configure strongly typed settings objects
        services.Configure<IdentitySettings>(configuration.GetSection(IdentitySettingsSectionName));

        services.TryAddScoped<ILoginTokenIssuer, JwtLoginTokenIssuer>();
        services.TryAddScoped<IPendingLoginStore, JwtPendingLoginStore>();

        return services;
    }

    //აპის JWT ავტორიზაცია IdentitySettings-ის JwtSecret, JwtIssuer და JwtAudience პარამეტრებით
    public static IServiceCollection AddCarcassJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        // configure jwt authentication
        IdentitySettings identitySettings =
            configuration.GetSection(IdentitySettingsSectionName).Get<IdentitySettings>() ??
            throw new Exception("IdentitySettings is null");
        string jwtSecret = identitySettings.JwtSecret ?? throw new Exception("JwtSecret is null");
        string jwtIssuer = identitySettings.JwtIssuer ?? throw new Exception("JwtIssuer is null");
        string jwtAudience = identitySettings.JwtAudience ?? throw new Exception("JwtAudience is null");
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
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience
            };
        });

        return services;
    }

    //ელფოსტის გაგზავნა: Development გარემოში SMTP სერვერის გარეშე წერილები ლოგში იწერება,
    //სხვა გარემოში SMTP სერვერის მითითება სავალდებულოა და აპლიკაცია გაშვებისთანავე შეჩერდება, თუ ის არ არის
    public static IServiceCollection AddCarcassEmailSending(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EmailSettings>().Bind(configuration.GetSection("EmailSettings"))
            .Validate<IHostEnvironment>(
                (settings, environment) =>
                    environment.IsDevelopment() || !string.IsNullOrWhiteSpace(settings.SmtpServer),
                "EmailSettings:SmtpServer must be configured outside the Development environment")
            .ValidateOnStart();
        services.AddScoped<IEmailSender>(serviceProvider =>
        {
            IOptions<EmailSettings> emailSettings = serviceProvider.GetRequiredService<IOptions<EmailSettings>>();
            return string.IsNullOrWhiteSpace(emailSettings.Value.SmtpServer)
                ? new LoggingEmailSender(serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Logging.ILogger<LoggingEmailSender>>())
                : new SmtpEmailSender(emailSettings,
                    serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SmtpEmailSender>>());
        });

        return services;
    }

    //ავტორიზაციის endpoint-ების დაცვა: ერთი IP მისამართიდან წუთში შეზღუდული რაოდენობის მოთხოვნა
    public static IServiceCollection AddCarcassAuthenticationRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthenticationRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = AuthenticationRequestsPerMinutePerIp,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    public static bool UseAuthenticationAndAuthorization(this IApplicationBuilder app, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseAuthenticationAndAuthorization));

        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        debugLogger?.Information("{MethodName} Finished", nameof(UseAuthenticationAndAuthorization));

        return true;
    }
}
