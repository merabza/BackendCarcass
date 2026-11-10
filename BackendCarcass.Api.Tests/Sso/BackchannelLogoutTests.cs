using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BackendCarcass.Api.Tests.Sso.Infrastructure;
using Xunit;

namespace BackendCarcass.Api.Tests.Sso;

//back-channel logout-ის მიღება (D18): ვალიდური ტოკენი სესიას მაშინვე ხურავს, ყალბი ან არასწორი — 400
public sealed class BackchannelLogoutTests
{
    private static async Task<HttpResponseMessage> PostLogoutTokenAsync(SsoTestHost host, string token)
    {
        using SsoBrowser sso = host.NewBrowser();
        using var form = new FormUrlEncodedContent(new Dictionary<string, string> { ["logout_token"] = token });
        return await sso.SendAsync(HttpMethod.Post, "/bff/backchannel", content: form);
    }

    private static async Task<(SsoBrowser Browser, SsoTestUser User)> SignedInAsync(SsoTestHost host)
    {
        SsoTestUser user = SsoTestUser.New();
        SsoBrowser browser = host.NewBrowser();
        using HttpResponseMessage response = await browser.SignInAsync(user);
        return (browser, user);
    }

    [Fact]
    public async Task ValidToken_EndsTheSessionOfItsSid()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        (SsoBrowser browser, SsoTestUser user) = await SignedInAsync(host);
        using SsoBrowser _ = browser;

        using HttpResponseMessage logout =
            await PostLogoutTokenAsync(host, host.Sso.CreateLogoutToken(user.SessionId, user.Subject));
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task TokenWithoutSid_EndsAllSessionsOfTheUser()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        SsoTestUser user = SsoTestUser.New();
        using SsoBrowser first = host.NewBrowser();
        using SsoBrowser second = host.NewBrowser();
        await first.SignInAsync(user);
        await second.SignInAsync(user with { SessionId = Guid.NewGuid().ToString() });

        using HttpResponseMessage logout = await PostLogoutTokenAsync(host, host.Sso.CreateLogoutToken(null, user.Subject));
        using HttpResponseMessage firstMe = await first.GetAsync(SsoTestHost.MeRoute);
        using HttpResponseMessage secondMe = await second.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, firstMe.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondMe.StatusCode);
    }

    [Fact]
    public async Task ReplayedToken_IsRefused()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        string token = host.Sso.CreateLogoutToken("made-up-sid", null);

        using HttpResponseMessage first = await PostLogoutTokenAsync(host, token);
        using HttpResponseMessage second = await PostLogoutTokenAsync(host, token);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    public static TheoryData<string> InvalidTokens()
    {
        return
        [
            "forged-signature", "expired", "other-audience", "other-issuer", "with-nonce", "without-events",
            "without-sub-and-sid",
            "not-a-token"
        ];
    }

    [Theory]
    [MemberData(nameof(InvalidTokens))]
    public async Task InvalidToken_IsRefused_AndTheSessionStays(string kind)
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        (SsoBrowser browser, SsoTestUser user) = await SignedInAsync(host);
        using SsoBrowser _ = browser;
        FakeSsoServer sso = host.Sso;
        string token = kind switch
        {
            "forged-signature" => sso.CreateLogoutToken(user.SessionId, user.Subject,
                new LogoutTokenOptions { Key = sso.ForeignKey }),
            "expired" => sso.CreateLogoutToken(user.SessionId, user.Subject,
                new LogoutTokenOptions { IssuedAt = DateTime.UtcNow.AddMinutes(-10) }),
            "other-audience" => sso.CreateLogoutToken(user.SessionId, user.Subject,
                new LogoutTokenOptions { Audience = "app-grammar" }),
            "other-issuer" => sso.CreateLogoutToken(user.SessionId, user.Subject,
                new LogoutTokenOptions { Issuer = "https://other-sso.test/" }),
            "with-nonce" => sso.CreateLogoutToken(user.SessionId, user.Subject,
                new LogoutTokenOptions { WithNonce = true }),
            "without-events" => sso.CreateLogoutToken(user.SessionId, user.Subject,
                new LogoutTokenOptions { WithEvents = false }),
            "without-sub-and-sid" => sso.CreateLogoutToken(null, null),
            _ => "not-a-token"
        };

        using HttpResponseMessage logout = await PostLogoutTokenAsync(host, token);
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.BadRequest, logout.StatusCode);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }
}
