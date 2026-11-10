using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BackendCarcass.Api.Tests.Sso.Infrastructure;
using BackendCarcassShared.Contracts.V1.Responses;
using Xunit;

namespace BackendCarcass.Api.Tests.Sso;

//API cookie-ით: CSRF-ის header, 401 გადამისამართების ნაცვლად, /bff/user, SignalR, გასვლა და ტოკენის განახლება
public sealed class BffApiTests
{
    private static async Task<SsoBrowser> SignedInAsync(SsoTestHost host, SsoTestUser? user = null)
    {
        SsoBrowser browser = host.NewBrowser();
        using HttpResponseMessage response = await browser.SignInAsync(user ?? SsoTestUser.New());
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return browser;
    }

    [Fact]
    public async Task Api_Answers401WithoutARedirect_WithoutASession()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = host.NewBrowser();

        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);
        using HttpResponseMessage user = await browser.GetAsync("/bff/user");

        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, user.StatusCode);
        Assert.Null(me.Headers.Location);
    }

    [Fact]
    public async Task Api_RefusesACookieRequestWithoutTheCsrfHeader()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = await SignedInAsync(host);

        using HttpResponseMessage withoutHeader = await browser.GetAsync(SsoTestHost.MeRoute, false);
        using HttpResponseMessage userWithoutHeader = await browser.GetAsync("/bff/user", false);
        using HttpResponseMessage withHeader = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, withoutHeader.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, userWithoutHeader.StatusCode);
        Assert.Equal(HttpStatusCode.OK, withHeader.StatusCode);
    }

    [Fact]
    public async Task BffUser_ReturnsTheProfileRolesAndTheLogoutToken()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        await host.AddLocalUserAsync("admin", "admin@example.com", null, SsoTestHost.AdminRole);
        using SsoBrowser browser = await SignedInAsync(host, SsoTestUser.New("admin", "admin@example.com"));

        using HttpResponseMessage response = await browser.GetAsync("/bff/user");
        BffUserResponse? user = await response.Content.ReadFromJsonAsync<BffUserResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("admin", user!.UserName);
        Assert.Equal("admin@example.com", user.Email);
        Assert.Equal(SsoTestHost.AdminRole, user.RoleName);
        Assert.False(string.IsNullOrEmpty(user.LogoutToken));
        Assert.False(string.IsNullOrEmpty(user.LogoutFormFieldName));
        Assert.DoesNotContain("access_token", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task SignalR_NegotiatesWithTheCookieAndTheCsrfHeader()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = await SignedInAsync(host);
        const string negotiate = SsoTestHost.HubRoute + "/negotiate?negotiateVersion=1";

        using HttpResponseMessage withHeader = await browser.SendAsync(HttpMethod.Post, negotiate, true);
        using HttpResponseMessage withoutHeader = await browser.SendAsync(HttpMethod.Post, negotiate);
        using SsoBrowser anonymous = host.NewBrowser();
        using HttpResponseMessage withoutSession = await anonymous.SendAsync(HttpMethod.Post, negotiate, true);

        Assert.Equal(HttpStatusCode.OK, withHeader.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, withoutHeader.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, withoutSession.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesTheRefreshTokenEndsTheSessionAndRedirectsToSsoEndSession()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = await SignedInAsync(host);
        using HttpResponseMessage userResponse = await browser.GetAsync("/bff/user");
        BffUserResponse user = (await userResponse.Content.ReadFromJsonAsync<BffUserResponse>())!;
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            [user.LogoutFormFieldName!] = user.LogoutToken!
        });

        using HttpResponseMessage logout = await browser.SendAsync(HttpMethod.Post, "/bff/logout", content: form);
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        string location = logout.Headers.Location!.ToString();
        Assert.StartsWith(FakeSsoServer.Issuer + "connect/endsession", location, StringComparison.Ordinal);
        Assert.Contains("id_token_hint=", location, StringComparison.Ordinal);
        Assert.Contains("post_logout_redirect_uri=", location, StringComparison.Ordinal);
        Assert.Equal(1, host.Sso.RevokeCalls);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task Logout_IsRefused_WithoutTheAntiforgeryToken()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = await SignedInAsync(host);
        using HttpResponseMessage userResponse = await browser.GetAsync("/bff/user");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>());

        using HttpResponseMessage logout = await browser.SendAsync(HttpMethod.Post, "/bff/logout", true, form);
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.BadRequest, logout.StatusCode);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal(0, host.Sso.RevokeCalls);
    }

    [Fact]
    public async Task Refresh_RenewsTheAccessToken_AndReReadsTheRoles()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        SsoTestUser ssoUser = SsoTestUser.New("grows");
        using SsoBrowser browser = await SignedInAsync(host, ssoUser);
        await host.AddRoleToUserAsync("grows", SsoTestHost.AdminRole);

        using HttpResponseMessage before = await browser.GetAsync(SsoTestHost.MeRoute);
        host.Time.Advance(TimeSpan.FromMinutes(9));
        using HttpResponseMessage after = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.EndsWith("|grows|", await before.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Equal(1, host.Sso.RefreshCalls);
        Assert.EndsWith("|grows|" + SsoTestHost.AdminRole, await after.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Refresh_EndsTheSession_WhenSsoRefusesTheRefreshToken()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = await SignedInAsync(host);
        host.Sso.RefreshStatus = HttpStatusCode.BadRequest;

        host.Time.Advance(TimeSpan.FromMinutes(9));
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task Refresh_KeepsTheSessionDuringTheGracePeriod_WhenSsoIsUnavailable()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = await SignedInAsync(host);
        host.Sso.RefreshStatus = HttpStatusCode.ServiceUnavailable;

        host.Time.Advance(TimeSpan.FromMinutes(20));
        using HttpResponseMessage inGrace = await browser.GetAsync(SsoTestHost.MeRoute);
        host.Time.Advance(TimeSpan.FromMinutes(25));
        using HttpResponseMessage afterGrace = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal(HttpStatusCode.OK, inGrace.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, afterGrace.StatusCode);
    }
}
