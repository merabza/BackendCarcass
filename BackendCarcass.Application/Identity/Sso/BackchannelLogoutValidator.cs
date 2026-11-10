using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace BackendCarcass.Application.Identity.Sso;

//back-channel logout-ის ტოკენის შემოწმება (OpenID Connect Back-Channel Logout 1.0, sso-design.md D18):
//ხელმოწერა sso-ს JWKS-ით, typ, iss, aud, iat/exp, events, nonce-ის არარსებობა, sub ან sid, jti-ს განმეორება
public sealed class BackchannelLogoutValidator : IDisposable
{
    //სპეციფიკაციის იდენტიფიკატორია და არა მისამართი, რომელსაც ვინმე გამოიძახებს
#pragma warning disable S5332
    public const string BackchannelLogoutEvent = "http://schemas.openid.net/event/backchannel-logout";
#pragma warning restore S5332

    private const string LogoutTokenType = "logout+jwt";
    private const string EventsClaim = "events";
    private const string NonceClaim = "nonce";

    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(1);

    private readonly MemoryCache _seenTokenIds = new(new MemoryCacheOptions());
    private readonly JsonWebTokenHandler _handler = new() { MapInboundClaims = false };
    private readonly ILogger<BackchannelLogoutValidator> _logger;
    private readonly IOptionsMonitor<OpenIdConnectOptions> _oidcOptions;

    public BackchannelLogoutValidator(IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
        ILogger<BackchannelLogoutValidator> logger)
    {
        _oidcOptions = oidcOptions;
        _logger = logger;
    }

    public void Dispose()
    {
        _seenTokenIds.Dispose();
    }

    //აბრუნებს გასაუქმებელი სესიის sid-ს და sub-ს, ან null-ს, თუ ტოკენი არავალიდურია
    public async Task<BackchannelLogoutSubject?> ValidateAsync(string? logoutToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(logoutToken))
        {
            return null;
        }

        OpenIdConnectOptions options = _oidcOptions.Get(CarcassSsoDefaults.OpenIdConnectScheme);
        OpenIdConnectConfiguration configuration =
            await options.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
        var parameters = new TokenValidationParameters
        {
            ValidIssuer = configuration.Issuer,
            ValidAudience = options.ClientId,
            IssuerSigningKeys = configuration.SigningKeys,
            ValidTypes = [LogoutTokenType],
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = ClockSkew
        };

        TokenValidationResult result = await _handler.ValidateTokenAsync(logoutToken, parameters);
        if (!result.IsValid || result.SecurityToken is not JsonWebToken token)
        {
            _logger.LogWarning(result.Exception, "Back-channel logout token refused");
            return null;
        }

        string? sid = token.Claims.FirstOrDefault(c => c.Type == SsoClaimTypes.SessionId)?.Value;
        string? sub = token.Subject;
        if (!HasLogoutEvent(token) || token.Claims.Any(c => c.Type == NonceClaim) ||
            token.IssuedAt == DateTime.MinValue || token.IssuedAt > DateTime.UtcNow + ClockSkew ||
            string.IsNullOrEmpty(sid) && string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(token.Id))
        {
            _logger.LogWarning("Back-channel logout token refused: required claims are missing or wrong");
            return null;
        }

        //ერთი და იგივე ტოკენი მეორედ არ მიიღება, სანამ ის ვადაშია
        if (_seenTokenIds.TryGetValue(token.Id, out _))
        {
            _logger.LogWarning("Back-channel logout token refused: jti was already used");
            return null;
        }

        _seenTokenIds.Set(token.Id, true, new DateTimeOffset(token.ValidTo, TimeSpan.Zero) + ClockSkew);
        return new BackchannelLogoutSubject(string.IsNullOrEmpty(sid) ? null : sid,
            string.IsNullOrEmpty(sub) ? null : sub);
    }

    private static bool HasLogoutEvent(JsonWebToken token)
    {
        Claim? events = token.Claims.FirstOrDefault(c => c.Type == EventsClaim);
        if (events is null)
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(events.Value);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                   document.RootElement.TryGetProperty(BackchannelLogoutEvent, out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public sealed record BackchannelLogoutSubject(string? SessionId, string? Subject);
