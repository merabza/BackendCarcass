using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Identity.Sso;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Domain.Users;
using BackendCarcassShared.Contracts.V1.Responses;
using BackendCarcassShared.Contracts.V1.Routes;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace BackendCarcass.Api.Endpoints.V1;

//SSO რეჟიმის (BFF) endpoint-ები აპის ჰოსტზე (sso-design.md, D17, D18)
// ReSharper disable once UnusedType.Global
public static class BffEndpoints
{
    private const string LogoutTokenField = "logout_token";
    private const string RefreshTokenName = "refresh_token";

    public static bool UseBffEndpoints(this IEndpointRouteBuilder endpoints, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseBffEndpoints));

        RouteGroupBuilder group = endpoints.MapGroup(BffRoutes.BffBase);

        group.MapGet(BffRoutes.Login, Login).AllowAnonymous();
        group.MapPost(BffRoutes.Logout, Logout).AllowAnonymous();
        group.MapGet(BffRoutes.User, User).RequireAuthorization();
        group.MapPost(BffRoutes.Backchannel, Backchannel).AllowAnonymous();

        debugLogger?.Information("{MethodName} Finished", nameof(UseBffEndpoints));

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> შესვლა sso.ltg.ge-ით
    //მოქმედება -> ბრაუზერი sso-ზე გადადის (code + PKCE); დაბრუნების შემდეგ — returnUrl-ზე, თუ ის ლოკალურია, სხვა
    //   შემთხვევაში მთავარ გვერდზე
    // GET /bff/login?returnUrl=
    private static ChallengeHttpResult Login(string? returnUrl)
    {
        return TypedResults.Challenge(new AuthenticationProperties { RedirectUri = LocalOrRoot(returnUrl) },
            [CarcassSsoDefaults.OpenIdConnectScheme]);
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> გასვლა ყველა აპიდან
    //შემავალი ინფორმაცია -> ფორმა antiforgery ტოკენით (ის /bff/user-ის პასუხშია)
    //მოქმედება -> განახლების ტოკენი უქმდება, აპის სესია იხურება და ბრაუზერი sso-ს end-session-ზე გადადის,
    //   საიდანაც sso დანარჩენ აპებს back-channel logout-ით ატყობინებს
    // POST /bff/logout
    private static async Task<Results<SignOutHttpResult, RedirectHttpResult, BadRequest>> Logout(
        HttpContext context, IAntiforgery antiforgery, SsoTokenClient tokenClient,
        CancellationToken cancellationToken = default)
    {
        if (!await antiforgery.IsRequestValidAsync(context))
        {
            return TypedResults.BadRequest();
        }

        AuthenticateResult session = await context.AuthenticateAsync(CarcassSsoDefaults.CookieScheme);
        if (!session.Succeeded)
        {
            return TypedResults.Redirect("/");
        }

        string? refreshToken = session.Properties.GetTokenValue(RefreshTokenName);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await tokenClient.RevokeAsync(refreshToken, cancellationToken);
        }

        return TypedResults.SignOut(new AuthenticationProperties { RedirectUri = "/" },
            [CarcassSsoDefaults.CookieScheme, CarcassSsoDefaults.OpenIdConnectScheme]);
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებელი, მისი როლები და აპის უფლებები; ცვლის LoginResponse-ს
    //მოქმედება -> სესიის გარეშე 401; პასუხში გასვლის ფორმის antiforgery ტოკენიცაა
    // GET /bff/user
    private static async Task<Results<Ok<BffUserResponse>, UnauthorizedHttpResult>> User(HttpContext context,
        ICurrentUser currentUser, SsoUserService users, IUserClaimsRepository userClaims, IAntiforgery antiforgery,
        CancellationToken cancellationToken = default)
    {
        User? user = await users.GetUserAsync(currentUser.Id, cancellationToken);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context);
        return TypedResults.Ok(new BffUserResponse
        {
            UserId = user.UsrId,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = string.Join(", ", currentUser.Roles),
            AppClaims = await userClaims.UserAppClaims(user.UserName, cancellationToken),
            LogoutToken = tokens.RequestToken,
            LogoutFormFieldName = tokens.FormFieldName
        });
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> back-channel logout-ის მიღება sso.ltg.ge-იდან (სერვერიდან სერვერზე)
    //შემავალი ინფორმაცია -> ფორმის ველი logout_token
    //მოქმედება -> ტოკენი მოწმდება და ამ sso-სესიის (sid), ან, sid-ის გარეშე, ამ მომხმარებლის (sub) ყველა სესია
    //   მაშინვე უქმდება. არავალიდური ტოკენი — 400
    // POST /bff/backchannel
    private static async Task<Results<Ok, BadRequest>> Backchannel(HttpContext context,
        BackchannelLogoutValidator validator, InMemoryTicketStore ticketStore, ILogger<BackchannelLogoutValidator> logger,
        CancellationToken cancellationToken = default)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!context.Request.HasFormContentType)
        {
            return TypedResults.BadRequest();
        }

        IFormCollection form = await context.Request.ReadFormAsync(cancellationToken);
        BackchannelLogoutSubject? subject = await validator.ValidateAsync(form[LogoutTokenField], cancellationToken);
        if (subject is null)
        {
            return TypedResults.BadRequest();
        }

        int removed = subject.SessionId is not null
            ? ticketStore.RemoveBySid(subject.SessionId)
            : ticketStore.RemoveBySub(subject.Subject!);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Back-channel logout ended {Count} session(s)", removed);
        }

        return TypedResults.Ok();
    }

    //მხოლოდ ლოკალური გზა (open redirect-ის წინააღმდეგ): "/x", მაგრამ არა "//host" ან "/\host"
    private static string LocalOrRoot(string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl) || returnUrl[0] != '/')
        {
            return "/";
        }

        if (returnUrl.Length > 1 && (returnUrl[1] == '/' || returnUrl[1] == '\\'))
        {
            return "/";
        }

        foreach (char c in returnUrl)
        {
            if (char.IsControl(c))
            {
                return "/";
            }
        }

        return returnUrl;
    }
}
