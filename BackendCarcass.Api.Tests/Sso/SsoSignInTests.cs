using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BackendCarcass.Api.Tests.Sso.Infrastructure;
using BackendCarcass.Application.Identity.Sso;
using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendCarcass.Api.Tests.Sso;

//შესვლა sso-ით: გადამისამართება PKCE-ით, callback, cookie-სესია, JIT და სინქრონიზაცია (D12, D13, D17)
public sealed class SsoSignInTests
{
    private static Task<User?> FindBySubjectAsync(SsoTestHost host, string subject)
    {
        return host.WithDbAsync(db => db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.SsoSubject == subject));
    }

    [Fact]
    public async Task Login_RedirectsToSsoWithCodeAndPkce()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = host.NewBrowser();

        using HttpResponseMessage response = await browser.SendAsync(HttpMethod.Get, "/bff/login?returnUrl=/texts");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Uri location = response.Headers.Location!;
        Assert.StartsWith(FakeSsoServer.Issuer + "connect/authorize", location.ToString(), StringComparison.Ordinal);
        Assert.Contains("response_type=code", location.Query, StringComparison.Ordinal);
        Assert.Contains("code_challenge_method=S256", location.Query, StringComparison.Ordinal);
        Assert.Contains("code_challenge=", location.Query, StringComparison.Ordinal);
        Assert.Contains("client_id=" + FakeSsoServer.ClientId, location.Query, StringComparison.Ordinal);
        Assert.Contains("api.errata", Uri.UnescapeDataString(location.Query), StringComparison.Ordinal);
        Assert.DoesNotContain("client_secret", location.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SignIn_CreatesTheSessionCookieAndReturnsToTheLocalReturnUrl()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = host.NewBrowser();

        using HttpResponseMessage response = await browser.SignInAsync(SsoTestUser.New(), "/texts?page=2");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/texts?page=2", response.Headers.Location!.ToString());
        Assert.True(host.Sso.PkceVerified);
        Assert.True(browser.HasCookie(SsoTestHost.CookieName));
        //ტოკენები cookie-ში არ არის: ის მხოლოდ სერვერული სესიის გასაღებს შეიცავს
        string cookie = browser.Cookies.GetCookies(SsoBrowser.BaseAddress)[SsoTestHost.CookieName]!.Value;
        Assert.True(cookie.Length < 600);
        string setCookie = string.Join(';', response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("//evil.example/")]
    [InlineData("/\\evil.example/")]
    public async Task SignIn_ReturnsToTheRoot_WhenTheReturnUrlIsNotLocal(string target)
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = host.NewBrowser();

        using HttpResponseMessage response = await browser.SignInAsync(SsoTestUser.New(), target);

        Assert.Equal("/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task SignIn_Fails_WhenTheCallbackStateIsForged()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = host.NewBrowser();
        using HttpResponseMessage challenge = await browser.SendAsync(HttpMethod.Get, "/bff/login");

        using HttpResponseMessage response =
            await browser.SendAsync(HttpMethod.Get, "/signin-oidc?code=made-up&state=made-up");

        Assert.NotEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.False(browser.HasCookie(SsoTestHost.CookieName));
    }

    //კოდი ერთჯერადია: იგივე callback მეორედ სესიას ვეღარ ქმნის
    [Fact]
    public async Task SignIn_Fails_WhenTheCallbackIsReplayed()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = host.NewBrowser();
        SsoTestUser user = SsoTestUser.New();
        using HttpResponseMessage challenge = await browser.SendAsync(HttpMethod.Get, "/bff/login");
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(challenge.Headers.Location!.Query);
        string code = host.Sso.IssueCode(user, query["nonce"].ToString(), query["code_challenge"].ToString(),
            query["redirect_uri"].ToString());
        string callback = "/signin-oidc?code=" + code + "&state=" + Uri.EscapeDataString(query["state"].ToString());
        using HttpResponseMessage first = await browser.SendAsync(HttpMethod.Get, callback);
        using SsoBrowser attacker = host.NewBrowser();
        attacker.Cookies.Add(browser.Cookies.GetAllCookies());

        using HttpResponseMessage replay = await attacker.SendAsync(HttpMethod.Get, callback);

        Assert.Equal(HttpStatusCode.Redirect, first.StatusCode);
        Assert.NotEqual(HttpStatusCode.Redirect, replay.StatusCode);
    }

    [Fact]
    public async Task SignIn_CreatesALocalUserWithoutRoles_OnTheFirstSignIn()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        using SsoBrowser browser = host.NewBrowser();
        SsoTestUser ssoUser = SsoTestUser.New("newcomer");

        await browser.SignInAsync(ssoUser);
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        User? user = await FindBySubjectAsync(host, ssoUser.Subject);
        Assert.NotNull(user);
        Assert.Equal("newcomer", user.UserName);
        Assert.Equal(CarcassSsoDefaults.UnusableHashMarker, user.PasswordHash);
        Assert.True(user.EmailConfirmed);
        Assert.Equal($"{user.UsrId}|newcomer|", await me.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SignIn_LinksTheLocalUserByVerifiedEmail_AndKeepsItsRoles()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        User local = await host.AddLocalUserAsync("olduser", "old@example.com", null, SsoTestHost.AdminRole);
        using SsoBrowser browser = host.NewBrowser();
        SsoTestUser ssoUser = SsoTestUser.New("olduser", "old@example.com");

        await browser.SignInAsync(ssoUser);
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        Assert.Equal($"{local.UsrId}|olduser|{SsoTestHost.AdminRole}", await me.Content.ReadAsStringAsync());
        Assert.Equal(local.UsrId, (await FindBySubjectAsync(host, ssoUser.Subject))!.UsrId);
    }

    [Fact]
    public async Task SignIn_Fails_WhenTheEmailIsTakenLocallyButNotVerifiedBySso()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        await host.AddLocalUserAsync("olduser", "old@example.com");
        using SsoBrowser browser = host.NewBrowser();
        SsoTestUser ssoUser = SsoTestUser.New("someone", "old@example.com") with { EmailVerified = false };

        using HttpResponseMessage response = await browser.SignInAsync(ssoUser);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.False(browser.HasCookie(SsoTestHost.CookieName));
        Assert.Null(await FindBySubjectAsync(host, ssoUser.Subject));
    }

    [Fact]
    public async Task SignIn_Fails_WhenTheEmailBelongsToAnotherSsoAccount()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        await host.AddLocalUserAsync("olduser", "old@example.com", Guid.NewGuid().ToString());
        using SsoBrowser browser = host.NewBrowser();

        using HttpResponseMessage response = await browser.SignInAsync(SsoTestUser.New("someone", "old@example.com"));

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.False(browser.HasCookie(SsoTestHost.CookieName));
    }

    [Fact]
    public async Task SignIn_AppendsASuffix_WhenTheUserNameIsTakenLocally()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        await host.AddLocalUserAsync("tester", "local-tester@example.com");
        await host.AddLocalUserAsync("tester-2", "local-tester2@example.com");
        using SsoBrowser browser = host.NewBrowser();
        SsoTestUser ssoUser = SsoTestUser.New("tester", "sso-tester@example.com");

        await browser.SignInAsync(ssoUser);

        Assert.Equal("tester-3", (await FindBySubjectAsync(host, ssoUser.Subject))!.UserName);
    }

    [Fact]
    public async Task SignIn_SyncsTheProfile_AndRenamesTheUserInManyToManyJoins()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        SsoTestUser ssoUser = SsoTestUser.New("before", "before@example.com");
        User local = await host.AddLocalUserAsync("before", "before@example.com", ssoUser.Subject,
            SsoTestHost.AdminRole);
        using SsoBrowser browser = host.NewBrowser();

        await browser.SignInAsync(ssoUser with
        {
            UserName = "after", Email = "after@example.com", FirstName = "ახალი", LastName = "გვარი"
        });
        using HttpResponseMessage me = await browser.GetAsync(SsoTestHost.MeRoute);

        User synced = (await FindBySubjectAsync(host, ssoUser.Subject))!;
        Assert.Equal("after", synced.UserName);
        Assert.Equal("AFTER", synced.NormalizedUserName);
        Assert.Equal("AFTER@EXAMPLE.COM", synced.NormalizedEmail);
        Assert.Equal("ახალი გვარი", synced.FullName);
        Assert.Equal($"{local.UsrId}|after|{SsoTestHost.AdminRole}", await me.Content.ReadAsStringAsync());
        Assert.Equal(["after"],
            await host.WithDbAsync(db => db.ManyToManyJoins.Select(m => m.PKey).Where(k => k.StartsWith("after") || k.StartsWith("before") || k.StartsWith("BEFORE")).ToListAsync()));
    }

    [Fact]
    public async Task SignIn_KeepsTheLocalUserName_WhenTheNewOneIsTakenByAnotherUser()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        SsoTestUser ssoUser = SsoTestUser.New("mine", "mine@example.com");
        await host.AddLocalUserAsync("mine", "mine@example.com", ssoUser.Subject);
        await host.AddLocalUserAsync("taken", "taken@example.com");
        using SsoBrowser browser = host.NewBrowser();

        await browser.SignInAsync(ssoUser with { UserName = "taken" });

        Assert.Equal("mine", (await FindBySubjectAsync(host, ssoUser.Subject))!.UserName);
    }
}
