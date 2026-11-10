using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;
using JwtTokenValidatedContext = Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext;
using OidcTokenValidatedContext = Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext;

namespace BackendCarcass.Application.Identity.Sso;

//SSO რეჟიმი (Authentication:Mode = Sso): აპის ჰოსტი sso.ltg.ge-ის confidential client-ია BFF-ით (G7, D15–D17).
//ბრაუზერი მხოლოდ HttpOnly cookie-ს იღებს, ტოკენები სერვერის მეხსიერებაშია, მანქანური კლიენტი Bearer-ით შედის
public static class CarcassSsoDependencyInjection
{
    private const string HostCookiePrefix = "__Host-";

    public static IServiceCollection AddCarcassSsoAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(CarcassAuthenticationSettings.SectionName);
        CarcassAuthenticationSettings settings = section.Get<CarcassAuthenticationSettings>() ?? new();
        SsoClientSettings sso = settings.Sso;
        string authority = Required(sso.Authority, "Sso:Authority");
        string clientId = Required(sso.ClientId, "Sso:ClientId");
        string clientSecret = Required(sso.ClientSecret, "Sso:ClientSecret");
        string apiScope = Required(sso.ApiScope, "Sso:ApiScope");
        string cookieName = Required(settings.CookieName, "CookieName");

        //HTTP მხოლოდ localhost-ზე (წესი 13); __Host- სახელს Secure სჭირდება
        bool requireHttps = !new Uri(authority).IsLoopback;
        CookieSecurePolicy securePolicy = cookieName.StartsWith(HostCookiePrefix, StringComparison.Ordinal)
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;

        services.Configure<CarcassAuthenticationSettings>(section);
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddSingleton<InMemoryTicketStore>();
        services.AddSingleton<SsoSessionRefresher>();
        services.AddSingleton<BackchannelLogoutValidator>();
        services.AddSingleton<SsoTokenClient>();
        services.AddScoped<SsoUserService>();
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = cookieName + ".af";
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = securePolicy;
            options.Cookie.HttpOnly = true;
        });

        //AddIdentity თავის ნაგულისხმევ სქემებს აყენებს (Identity.Application), ამიტომ ყველა აქ ცალ-ცალკე იწერება
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CarcassSsoDefaults.PolicyScheme;
                options.DefaultAuthenticateScheme = CarcassSsoDefaults.PolicyScheme;
                options.DefaultChallengeScheme = CarcassSsoDefaults.PolicyScheme;
                options.DefaultForbidScheme = CarcassSsoDefaults.PolicyScheme;
                options.DefaultSignInScheme = CarcassSsoDefaults.CookieScheme;
                options.DefaultSignOutScheme = CarcassSsoDefaults.CookieScheme;
            })
            .AddPolicyScheme(CarcassSsoDefaults.PolicyScheme, CarcassSsoDefaults.PolicyScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.Authorization.ToString()
                        .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : CarcassSsoDefaults.CookieScheme;
            })
            .AddCookie(CarcassSsoDefaults.CookieScheme, options =>
            {
                options.Cookie.Name = cookieName;
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = securePolicy;
                options.ExpireTimeSpan = sso.SessionIdleTimeout;
                options.SlidingExpiration = true;
                //API-ს და /bff/user-ს გადამისამართება არ სჭირდება: SPA 401-ზე თვითონ გადადის /bff/login-ზე
                options.Events.OnRedirectToLogin = context => SetStatus(context.Response, StatusCodes.Status401Unauthorized);
                options.Events.OnRedirectToAccessDenied = context => SetStatus(context.Response, StatusCodes.Status403Forbidden);
                options.Events.OnValidatePrincipal = context =>
                    context.HttpContext.RequestServices.GetRequiredService<SsoSessionRefresher>()
                        .ValidatePrincipalAsync(context);
            })
            .AddOpenIdConnect(CarcassSsoDefaults.OpenIdConnectScheme, options =>
            {
                options.Authority = authority;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.RequireHttpsMetadata = requireHttps;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.ResponseMode = OpenIdConnectResponseMode.Query;
                options.UsePkce = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = false;
                options.MapInboundClaims = false;
                options.SignInScheme = CarcassSsoDefaults.CookieScheme;
                options.SignOutScheme = CarcassSsoDefaults.CookieScheme;
                options.CallbackPath = CarcassSsoDefaults.CallbackPath;
                options.SignedOutCallbackPath = CarcassSsoDefaults.SignedOutCallbackPath;
                options.SignedOutRedirectUri = "/";
                options.Scope.Clear();
                foreach (string scope in new[] { "openid", "profile", "email", "offline_access", apiScope }
                             .Concat(sso.Scopes).Distinct(StringComparer.Ordinal))
                {
                    options.Scope.Add(scope);
                }

                options.TokenValidationParameters.NameClaimType = SsoClaimTypes.PreferredUserName;
                //correlation და nonce cookie-ებსაც აპის პრეფიქსი აქვს (G13); __Host- სახელს Path=/ სჭირდება
                options.CorrelationCookie.Name = cookieName + ".corr.";
                options.NonceCookie.Name = cookieName + ".nonce.";
                foreach (CookieBuilder cookie in new[] { options.CorrelationCookie, options.NonceCookie })
                {
                    cookie.Path = "/";
                    cookie.SameSite = SameSiteMode.Lax;
                    cookie.SecurePolicy = securePolicy;
                    cookie.HttpOnly = true;
                }

                options.Events.OnTokenValidated = OnTokenValidated;
                options.Events.OnRemoteFailure = OnRemoteFailure;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authority;
                options.Audience = apiScope;
                options.RequireHttpsMetadata = requireHttps;
                options.MapInboundClaims = false;
                options.Events = new JwtBearerEvents { OnTokenValidated = OnMachineTokenValidated };
            });

        services.AddOptions<CookieAuthenticationOptions>(CarcassSsoDefaults.CookieScheme)
            .Configure<InMemoryTicketStore>((options, store) => options.SessionStore = store);

        return services;
    }

    //sso-დან დაბრუნებისას: id_token-ის მომხმარებელი → ლოკალური მომხმარებელი და მისი როლები (D12, D13)
    private static async Task OnTokenValidated(OidcTokenValidatedContext context)
    {
        SsoUserService users = context.HttpContext.RequestServices.GetRequiredService<SsoUserService>();
        Result<ClaimsPrincipal> principal = await users.SignInAsync(context.Principal!,
            CarcassSsoDefaults.CookieScheme, context.HttpContext.RequestAborted);
        if (principal.IsFailure)
        {
            context.Fail(principal.Error.Description);
            return;
        }

        context.Principal = principal.Value;
    }

    //შესვლა ვერ მოხერხდა (sso მიუწვდომელია, ან მომხმარებელი უარყოფილია): ქართული გვერდი (D24)
    private static async Task OnRemoteFailure(RemoteFailureContext context)
    {
        context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(CarcassSsoDependencyInjection)).LogWarning(context.Failure, "Sso sign-in failed");
        context.HandleResponse();
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(
            "<!doctype html><html lang=\"ka\"><head><meta charset=\"utf-8\"><title>შესვლა ვერ მოხერხდა</title></head>" +
            "<body><h1>შესვლა ვერ მოხერხდა</h1><p>შესვლის სერვისთან კავშირი ვერ დამყარდა, ან შესვლა უარყოფილია. " +
            "სცადეთ მოგვიანებით.</p><p><a href=\"/\">მთავარ გვერდზე დაბრუნება</a></p></body></html>",
            Encoding.UTF8, context.HttpContext.RequestAborted);
    }

    //Bearer მხოლოდ client_credentials-ის ტოკენს იღებს: sub = client_id და client_id კონფიგურაციაშია (D9, D17)
    private static async Task OnMachineTokenValidated(JwtTokenValidatedContext context)
    {
        CarcassAuthenticationSettings settings = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<CarcassAuthenticationSettings>>().Value;
        string? clientId = context.Principal?.FindFirstValue(SsoClaimTypes.ClientId);
        string? subject = context.Principal?.FindFirstValue(SsoClaimTypes.Subject);
        if (clientId is null || subject != clientId ||
            !settings.MachineClients.TryGetValue(clientId, out string? localUserName))
        {
            context.Fail("The token is not a token of a registered machine client");
            return;
        }

        SsoUserService users = context.HttpContext.RequestServices.GetRequiredService<SsoUserService>();
        Result<ClaimsPrincipal> principal = await users.MachineSignInAsync(clientId, localUserName,
            JwtBearerDefaults.AuthenticationScheme, context.HttpContext.RequestAborted);
        if (principal.IsFailure)
        {
            context.Fail(principal.Error.Description);
            return;
        }

        context.Principal = principal.Value;
    }

    private static Task SetStatus(HttpResponse response, int statusCode)
    {
        response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    private static string Required(string? value, string key)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException(
                $"{CarcassAuthenticationSettings.SectionName}:{key} must be configured in the Sso mode")
            : value;
    }
}
