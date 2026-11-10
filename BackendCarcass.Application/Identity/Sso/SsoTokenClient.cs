using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BackendCarcass.Application.Identity.Sso;

//sso.ltg.ge-ის token და revocation endpoint-ები აპის ჰოსტიდან (სერვერიდან სერვერზე). იყენებს OpenID Connect-ის
//handler-ის კონფიგურაციას და HttpClient-ს
public sealed class SsoTokenClient
{
    private const string RevocationEndpointKey = "revocation_endpoint";

    private readonly ILogger<SsoTokenClient> _logger;
    private readonly IOptionsMonitor<OpenIdConnectOptions> _oidcOptions;

    public SsoTokenClient(IOptionsMonitor<OpenIdConnectOptions> oidcOptions, ILogger<SsoTokenClient> logger)
    {
        _oidcOptions = oidcOptions;
        _logger = logger;
    }

    public async Task<SsoRefreshResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        OpenIdConnectOptions options = _oidcOptions.Get(CarcassSsoDefaults.OpenIdConnectScheme);
        try
        {
            OpenIdConnectConfiguration configuration =
                await options.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = options.ClientId!,
                ["client_secret"] = options.ClientSecret!
            });
            using HttpResponseMessage response =
                await options.Backchannel.PostAsync(new Uri(configuration.TokenEndpoint), content, cancellationToken);

            //4xx (invalid_grant, invalid_client): სესია დასრულებულია; 5xx: sso მიუწვდომელია
            if ((int)response.StatusCode >= (int)HttpStatusCode.InternalServerError)
            {
                _logger.LogWarning("Sso token endpoint answered {StatusCode} on refresh", (int)response.StatusCode);
                return SsoRefreshResult.Unavailable;
            }

            if (!response.IsSuccessStatusCode)
            {
                return SsoRefreshResult.Rejected;
            }

            TokenResponse? tokens = await JsonSerializer.DeserializeAsync<TokenResponse>(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return string.IsNullOrEmpty(tokens?.AccessToken)
                ? SsoRefreshResult.Rejected
                : new SsoRefreshResult(ESsoRefreshOutcome.Refreshed, tokens.AccessToken, tokens.RefreshToken,
                    tokens.IdToken, tokens.ExpiresIn);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or InvalidOperationException
                                      or IOException)
        {
            _logger.LogWarning(e, "Sso token endpoint is unreachable on refresh");
            return SsoRefreshResult.Unavailable;
        }
    }

    //გასვლისას განახლების ტოკენის გაუქმება; შეცდომა გასვლას არ აჩერებს
    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        OpenIdConnectOptions options = _oidcOptions.Get(CarcassSsoDefaults.OpenIdConnectScheme);
        try
        {
            OpenIdConnectConfiguration configuration =
                await options.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
            if (!configuration.AdditionalData.TryGetValue(RevocationEndpointKey, out object? endpoint) ||
                endpoint is not string revocationEndpoint)
            {
                return;
            }

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = refreshToken,
                ["token_type_hint"] = "refresh_token",
                ["client_id"] = options.ClientId!,
                ["client_secret"] = options.ClientSecret!
            });
            using HttpResponseMessage response =
                await options.Backchannel.PostAsync(new Uri(revocationEndpoint), content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Sso revocation endpoint answered {StatusCode}", (int)response.StatusCode);
            }
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or InvalidOperationException
                                      or IOException)
        {
            _logger.LogWarning(e, "Refresh token revocation failed");
        }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
        [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
        [JsonPropertyName("id_token")] public string? IdToken { get; init; }
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
    }
}

public enum ESsoRefreshOutcome
{
    Refreshed,
    Rejected,
    Unavailable
}

public sealed record SsoRefreshResult(
    ESsoRefreshOutcome Outcome,
    string? AccessToken = null,
    string? RefreshToken = null,
    string? IdToken = null,
    int ExpiresIn = 0)
{
    public static SsoRefreshResult Rejected { get; } = new(ESsoRefreshOutcome.Rejected);
    public static SsoRefreshResult Unavailable { get; } = new(ESsoRefreshOutcome.Unavailable);
}
