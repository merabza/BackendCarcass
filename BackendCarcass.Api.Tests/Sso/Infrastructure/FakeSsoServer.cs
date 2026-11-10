using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity.Sso;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace BackendCarcass.Api.Tests.Sso.Infrastructure;

//sso.ltg.ge-ის იმიტაცია: discovery-ს კონფიგურაცია (ტესტის RSA გასაღებით), token და revocation endpoint-ები
//აპის ჰოსტის HttpClient-ისთვის, და ტესტის ტოკენების (id_token, access token, logout token) შექმნა
internal sealed class FakeSsoServer : HttpMessageHandler
{
    public const string Issuer = "https://sso.test/";
    public const string ClientId = "app-errata";
    public const string ClientSecret = "made-up-client-secret";
    public const string ApiAudience = "api.errata";

    private readonly ConcurrentDictionary<string, PendingCode> _codes = new(StringComparer.Ordinal);
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly RSA _otherRsa = RSA.Create(2048);
    private readonly JsonWebTokenHandler _handler = new();

    public FakeSsoServer()
    {
        SigningKey = new RsaSecurityKey(_rsa) { KeyId = "test-key" };
        ForeignKey = new RsaSecurityKey(_otherRsa) { KeyId = "test-key" };
        Configuration = new OpenIdConnectConfiguration
        {
            Issuer = Issuer,
            AuthorizationEndpoint = Issuer + "connect/authorize",
            TokenEndpoint = Issuer + "connect/token",
            EndSessionEndpoint = Issuer + "connect/endsession"
        };
        Configuration.SigningKeys.Add(SigningKey);
        Configuration.AdditionalData["revocation_endpoint"] = Issuer + "connect/revoke";
    }

    public RsaSecurityKey SigningKey { get; }

    //sso-ს გასაღები არ არის: მისი ხელმოწერა ყალბია
    public RsaSecurityKey ForeignKey { get; }

    public OpenIdConnectConfiguration Configuration { get; }

    public int RefreshCalls { get; private set; }
    public int RevokeCalls { get; private set; }
    public bool PkceVerified { get; private set; }

    //განახლების პასუხი: null — ახალი ტოკენები, სხვა შემთხვევაში ეს სტატუსი
    public HttpStatusCode? RefreshStatus { get; set; }

    public string IssueCode(SsoTestUser user, string nonce, string codeChallenge, string redirectUri)
    {
        string code = Guid.NewGuid().ToString("N");
        _codes[code] = new PendingCode(user, nonce, codeChallenge, redirectUri);
        return code;
    }

    public string CreateIdToken(SsoTestUser user, string nonce)
    {
        var claims = new Dictionary<string, object>
        {
            [SsoClaimTypes.Subject] = user.Subject,
            [SsoClaimTypes.SessionId] = user.SessionId,
            ["nonce"] = nonce,
            [SsoClaimTypes.PreferredUserName] = user.UserName,
            [SsoClaimTypes.Email] = user.Email,
            [SsoClaimTypes.EmailVerified] = user.EmailVerified,
            [SsoClaimTypes.GivenName] = user.FirstName,
            [SsoClaimTypes.FamilyName] = user.LastName,
            [SsoClaimTypes.Name] = user.FirstName + " " + user.LastName
        };
        return CreateToken(ClientId, claims, TimeSpan.FromMinutes(10), null, SigningKey);
    }

    public string CreateAccessToken(string subject, string clientId, string audience, RsaSecurityKey? key = null)
    {
        var claims = new Dictionary<string, object>
        {
            [SsoClaimTypes.Subject] = subject, [SsoClaimTypes.ClientId] = clientId, ["scope"] = audience
        };
        return CreateToken(audience, claims, TimeSpan.FromMinutes(10), "at+jwt", key ?? SigningKey);
    }

    public string CreateLogoutToken(string? sid, string? sub, LogoutTokenOptions? options = null)
    {
        options ??= new LogoutTokenOptions();
        var claims = new Dictionary<string, object> { ["jti"] = options.Jti ?? Guid.NewGuid().ToString() };
        if (sid is not null)
        {
            claims[SsoClaimTypes.SessionId] = sid;
        }

        if (sub is not null)
        {
            claims[SsoClaimTypes.Subject] = sub;
        }

        if (options.WithEvents)
        {
            claims["events"] = new Dictionary<string, object>
            {
                [BackchannelLogoutValidator.BackchannelLogoutEvent] = new Dictionary<string, object>()
            };
        }

        if (options.WithNonce)
        {
            claims["nonce"] = "made-up-nonce";
        }

        return CreateToken(options.Audience ?? ClientId, claims, options.Lifetime ?? TimeSpan.FromMinutes(2),
            "logout+jwt", options.Key ?? SigningKey, options.IssuedAt, options.Issuer);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string path = request.RequestUri!.AbsolutePath;
        Dictionary<string, string> form = await ReadFormAsync(request, cancellationToken);
        if (path == "/connect/revoke")
        {
            RevokeCalls++;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        if (path != "/connect/token" || form.GetValueOrDefault("client_id") != ClientId ||
            form.GetValueOrDefault("client_secret") != ClientSecret)
        {
            return Error("invalid_client");
        }

        return form.GetValueOrDefault("grant_type") switch
        {
            "authorization_code" => ExchangeCode(form),
            "refresh_token" => Refresh(),
            _ => Error("unsupported_grant_type")
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _rsa.Dispose();
            _otherRsa.Dispose();
        }

        base.Dispose(disposing);
    }

    private HttpResponseMessage ExchangeCode(Dictionary<string, string> form)
    {
        //კოდი ერთჯერადია
        if (!_codes.TryRemove(form.GetValueOrDefault("code") ?? string.Empty, out PendingCode? pending) ||
            form.GetValueOrDefault("redirect_uri") != pending.RedirectUri)
        {
            return Error("invalid_grant");
        }

        string verifier = form.GetValueOrDefault("code_verifier") ?? string.Empty;
        string challenge = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        if (verifier.Length == 0 || challenge != pending.CodeChallenge)
        {
            return Error("invalid_grant");
        }

        PkceVerified = true;
        return Tokens(CreateIdToken(pending.User, pending.Nonce),
            CreateAccessToken(pending.User.Subject, ClientId, ApiAudience));
    }

    private HttpResponseMessage Refresh()
    {
        RefreshCalls++;
        return RefreshStatus is { } status
            ? new HttpResponseMessage(status) { Content = new StringContent("""{"error":"invalid_grant"}""") }
            : Tokens(null, "refreshed-access-token");
    }

    private static HttpResponseMessage Tokens(string? idToken, string accessToken)
    {
        var body = new Dictionary<string, object>
        {
            ["access_token"] = accessToken,
            ["refresh_token"] = Guid.NewGuid().ToString("N"),
            ["token_type"] = "Bearer",
            ["expires_in"] = 600
        };
        if (idToken is not null)
        {
            body["id_token"] = idToken;
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage Error(string error)
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent($$"""{"error":"{{error}}"}""", Encoding.UTF8, "application/json")
        };
    }

    private string CreateToken(string audience, Dictionary<string, object> claims, TimeSpan lifetime,
        string? tokenType, SecurityKey key, DateTime? issuedAt = null, string? issuer = null)
    {
        DateTime now = issuedAt ?? DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer ?? Issuer,
            Audience = audience,
            Claims = claims,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + lifetime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
        };
        if (tokenType is not null)
        {
            descriptor.TokenType = tokenType;
        }

        return _handler.CreateToken(descriptor);
    }

    private static async Task<Dictionary<string, string>> ReadFormAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>(StringComparer.Ordinal);
        if (request.Content is null)
        {
            return form;
        }

        string body = await request.Content.ReadAsStringAsync(cancellationToken);
        foreach (string pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = pair.Split('=', 2);
            form[Uri.UnescapeDataString(parts[0])] =
                parts.Length > 1 ? Uri.UnescapeDataString(parts[1].Replace('+', ' ')) : string.Empty;
        }

        return form;
    }

    private sealed record PendingCode(SsoTestUser User, string Nonce, string CodeChallenge, string RedirectUri);
}

internal sealed record LogoutTokenOptions
{
    public string? Audience { get; init; }
    public string? Jti { get; init; }
    public bool WithEvents { get; init; } = true;
    public bool WithNonce { get; init; }
    public TimeSpan? Lifetime { get; init; }
    public DateTime? IssuedAt { get; init; }
    public RsaSecurityKey? Key { get; init; }
    public string? Issuer { get; init; }
}

//sso-ს მომხმარებელი (სინთეზური მონაცემები)
internal sealed record SsoTestUser(string Subject, string SessionId, string UserName, string Email,
    bool EmailVerified = true, string FirstName = "Test", string LastName = "User")
{
    public static SsoTestUser New(string userName = "tester", string? email = null)
    {
        return new SsoTestUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), userName,
            email ?? userName + "@example.com");
    }
}
