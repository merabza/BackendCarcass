using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Api.Tests.Sso.Infrastructure;
using BackendCarcass.Application.Identity.Sso;
using Xunit;

namespace BackendCarcass.Api.Tests.Sso;

//Authentication:Mode: Local-ში ყველაფერი ისეა, როგორც აქამდე; Sso-ში ლოკალური შესვლისა და პროფილის
//endpoint-ები გამორთულია და /bff/* ჩნდება (G11)
public sealed class AuthenticationModeTests
{
    private static readonly string[] LocalOnly =
    [
        "/api/v1/authentication/login", "/api/v1/authentication/registration", "/api/v1/authentication/refresh",
        "/api/v1/userrights/changeprofile", "/api/v1/userrights/changepassword",
        "/api/v1/userrights/deletecurrentuser/{userName}"
    ];

    private static readonly string[] SsoOnly = ["/bff/login", "/bff/logout", "/bff/user", "/bff/backchannel"];

    [Fact]
    public async Task LocalMode_MapsTheLocalEndpoints_AndNoBff()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync(EAuthenticationMode.Local);

        List<string> routes = [.. host.RoutePatterns()];

        Assert.All(LocalOnly, route => Assert.Contains(route, routes));
        Assert.DoesNotContain(routes, route => SsoOnly.Contains(route));
        Assert.Contains("/api/v1/userrights/getmainmenu", routes);
    }

    [Fact]
    public async Task SsoMode_MapsBff_AndNoLocalSignInOrProfileEndpoints()
    {
        await using SsoTestHost host = await SsoTestHost.StartAsync();

        List<string> routes = [.. host.RoutePatterns()];

        Assert.All(SsoOnly, route => Assert.Contains(route, routes));
        Assert.DoesNotContain(routes, route => LocalOnly.Contains(route));
        Assert.Contains("/api/v1/userrights/getmainmenu", routes);
        Assert.Contains("/api/v1/userrights/iscurrentuservalid", routes);
    }
}
