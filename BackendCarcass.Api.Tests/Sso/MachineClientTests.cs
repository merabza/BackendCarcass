using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BackendCarcass.Api.Tests.Sso.Infrastructure;
using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendCarcass.Api.Tests.Sso;

//მანქანური კლიენტი Bearer-ით (D17, D23): მხოლოდ რეგისტრირებული client_credentials ტოკენი, სწორი audience-ით
public sealed class MachineClientTests
{
    private static async Task<HttpResponseMessage> CallWithBearerAsync(SsoTestHost host, string token)
    {
        using SsoBrowser client = host.NewBrowser();
        return await client.SendAsync(HttpMethod.Get, SsoTestHost.MeRoute, bearer: token);
    }

    [Fact]
    public async Task ClientCredentialsToken_ActsAsTheConfiguredTechnicalUser()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        int workerId = await host.WithDbAsync(db =>
            db.Users.Where(u => u.UserName == SsoTestHost.MachineUserName).Select(u => u.UsrId).SingleAsync());
        string token = host.Sso.CreateAccessToken(SsoTestHost.MachineClientId, SsoTestHost.MachineClientId,
            FakeSsoServer.ApiAudience);

        using HttpResponseMessage response = await CallWithBearerAsync(host, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"{workerId}|{SsoTestHost.MachineUserName}|{SsoTestHost.AdminRole}",
            await response.Content.ReadAsStringAsync());
    }

    public static TheoryData<string> RefusedTokens()
    {
        return ["other-audience", "unknown-client", "user-token", "forged-signature"];
    }

    [Theory]
    [MemberData(nameof(RefusedTokens))]
    public async Task OtherTokens_AreRefused(string kind)
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        FakeSsoServer sso = host.Sso;
        string token = kind switch
        {
            "other-audience" => sso.CreateAccessToken(SsoTestHost.MachineClientId, SsoTestHost.MachineClientId,
                "api.grammar"),
            "unknown-client" => sso.CreateAccessToken("other-worker", "other-worker", FakeSsoServer.ApiAudience),
            "user-token" => sso.CreateAccessToken(Guid.NewGuid().ToString(), FakeSsoServer.ClientId,
                FakeSsoServer.ApiAudience),
            _ => sso.CreateAccessToken(SsoTestHost.MachineClientId, SsoTestHost.MachineClientId,
                FakeSsoServer.ApiAudience, sso.ForeignKey)
        };

        using HttpResponseMessage response = await CallWithBearerAsync(host, token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ClientCredentialsToken_IsRefused_WhenTheTechnicalUserIsMissing()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();
        await host.WithDbAsync(db => db.Users.Where(u => u.UserName == SsoTestHost.MachineUserName)
            .ExecuteDeleteAsync());
        string token = host.Sso.CreateAccessToken(SsoTestHost.MachineClientId, SsoTestHost.MachineClientId,
            FakeSsoServer.ApiAudience);

        using HttpResponseMessage response = await CallWithBearerAsync(host, token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(await host.WithDbAsync(db =>
            db.Users.SingleOrDefaultAsync(u => u.UserName == SsoTestHost.MachineUserName)));
    }
}
