using System;
using System.Threading.Tasks;
using BackendCarcassShared.Contracts.V1.Routes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BackendCarcass.Application.Identity.Sso;

//SSO რეჟიმი (D17): cookie-ით ავტორიზებულ ყოველ /api/* და /bff/* მოთხოვნას X-CSRF: 1 header სჭირდება. ის CORS-ის
//preflight-ს აიძულებს, ასე რომ სხვა საიტი მოთხოვნას ვერ გამოგზავნის. WebSocket-ის handshake-ს header-ის დამატება
//არ შეუძლია, ამიტომ მასზე Origin მოწმდება. Bearer ტოკენით (მანქანური კლიენტი) მოთხოვნას cookie არ სჭირდება
public sealed class CarcassCsrfMiddleware
{
    private static readonly PathString Api = new("/api");
    private static readonly PathString Bff = new(BffRoutes.BffBase);
    private static readonly PathString BffLogin = new(BffRoutes.BffBase + BffRoutes.Login);
    private static readonly PathString BffLogout = new(BffRoutes.BffBase + BffRoutes.Logout);
    private static readonly PathString BffBackchannel = new(BffRoutes.BffBase + BffRoutes.Backchannel);

    private readonly string _cookieName;
    private readonly RequestDelegate _next;

    public CarcassCsrfMiddleware(RequestDelegate next, IOptions<CarcassAuthenticationSettings> settings)
    {
        _next = next;
        _cookieName = settings.Value.CookieName!;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (!NeedsCheck(context) || IsAllowed(context))
        {
            return _next(context);
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    private bool NeedsCheck(HttpContext context)
    {
        PathString path = context.Request.Path;
        if (!path.StartsWithSegments(Api) && !path.StartsWithSegments(Bff))
        {
            return false;
        }

        //შესვლა ბრაუზერის გადამისამართებაა, გასვლა ფორმაა antiforgery ტოკენით, back-channel-ს კი sso აგზავნის
        if (path.Equals(BffLogin) || path.Equals(BffLogout) || path.Equals(BffBackchannel))
        {
            return false;
        }

        return !context.Request.Headers.Authorization.ToString()
                   .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
               context.Request.Cookies.ContainsKey(_cookieName);
    }

    private static bool IsAllowed(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            string origin = context.Request.Headers.Origin.ToString();
            string own = context.Request.Scheme + "://" + context.Request.Host.Value;
            return string.Equals(origin, own, StringComparison.OrdinalIgnoreCase);
        }

        return context.Request.Headers[BffRoutes.CsrfHeaderName] == BffRoutes.CsrfHeaderValue;
    }
}
