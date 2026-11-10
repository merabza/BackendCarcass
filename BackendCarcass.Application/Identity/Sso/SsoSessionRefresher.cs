using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Identity.Sso;

//აპის სესიის ყოველ მოთხოვნაზე: თუ წვდომის ტოკენს 2 წუთზე ნაკლები დარჩა, ჰოსტი მას sso-ზე აახლებს და
//მომხმარებლის როლებს ბაზიდან თავიდან კითხულობს (D4, D16). sso-ს მიუწვდომლობისას სესია ძველი ტოკენით
//OutageGracePeriod-ის განმავლობაში რჩება (D24); უარის (invalid_grant) შემთხვევაში სესია მაშინვე იხურება
public sealed class SsoSessionRefresher
{
    private const string AccessTokenName = "access_token";
    private const string RefreshTokenName = "refresh_token";
    private const string IdTokenName = "id_token";
    private const string ExpiresAtName = "expires_at";

    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(2);

    private readonly ILogger<SsoSessionRefresher> _logger;
    private readonly IOptions<CarcassAuthenticationSettings> _settings;
    private readonly TimeProvider _timeProvider;

    public SsoSessionRefresher(IOptions<CarcassAuthenticationSettings> settings, TimeProvider timeProvider,
        ILogger<SsoSessionRefresher> logger)
    {
        _settings = settings;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        if (context.Principal is null)
        {
            return;
        }

        DateTimeOffset now = _timeProvider.GetUtcNow();
        if (!DateTimeOffset.TryParse(context.Properties.GetTokenValue(ExpiresAtName), CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out DateTimeOffset expiresAt))
        {
            await RejectAsync(context);
            return;
        }

        if (expiresAt - now > RefreshMargin)
        {
            return;
        }

        string? refreshToken = context.Properties.GetTokenValue(RefreshTokenName);
        if (string.IsNullOrEmpty(refreshToken))
        {
            await RejectAsync(context);
            return;
        }

        SsoTokenClient tokenClient = context.HttpContext.RequestServices.GetRequiredService<SsoTokenClient>();
        SsoRefreshResult refreshed = await tokenClient.RefreshAsync(refreshToken, context.HttpContext.RequestAborted);
        switch (refreshed.Outcome)
        {
            case ESsoRefreshOutcome.Refreshed:
                await ApplyAsync(context, refreshed, now);
                break;
            case ESsoRefreshOutcome.Unavailable when now < expiresAt + _settings.Value.Sso.OutageGracePeriod:
                _logger.LogWarning("Sso is unavailable, the session keeps its access token for the grace period");
                break;
            default:
                await RejectAsync(context);
                break;
        }
    }

    private async Task ApplyAsync(CookieValidatePrincipalContext context, SsoRefreshResult refreshed,
        DateTimeOffset now)
    {
        SsoUserService users = context.HttpContext.RequestServices.GetRequiredService<SsoUserService>();
        Result<ClaimsPrincipal> principal = await users.RefreshAsync(context.Principal!,
            CarcassSsoDefaults.CookieScheme, context.HttpContext.RequestAborted);
        if (principal.IsFailure)
        {
            await RejectAsync(context);
            return;
        }

        context.Properties.UpdateTokenValue(AccessTokenName, refreshed.AccessToken!);
        if (!string.IsNullOrEmpty(refreshed.RefreshToken))
        {
            context.Properties.UpdateTokenValue(RefreshTokenName, refreshed.RefreshToken);
        }

        if (!string.IsNullOrEmpty(refreshed.IdToken))
        {
            context.Properties.UpdateTokenValue(IdTokenName, refreshed.IdToken);
        }

        context.Properties.UpdateTokenValue(ExpiresAtName,
            now.AddSeconds(refreshed.ExpiresIn).ToString("o", CultureInfo.InvariantCulture));
        context.ReplacePrincipal(principal.Value);
        context.ShouldRenew = true;
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CarcassSsoDefaults.CookieScheme);
    }
}
