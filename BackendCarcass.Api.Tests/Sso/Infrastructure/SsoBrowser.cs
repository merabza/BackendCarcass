using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BackendCarcassShared.Contracts.V1.Routes;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace BackendCarcass.Api.Tests.Sso.Infrastructure;

//ბრაუზერის იმიტაცია TestServer-ისთვის: cookie-ებს ინახავს და აგზავნის, გადამისამართებას არ მიჰყვება
internal sealed class SsoBrowser : IDisposable
{
    public static readonly Uri BaseAddress = new("http://localhost/");

    private readonly HttpClient _client;
    private readonly SsoTestHost _host;

    public SsoBrowser(SsoTestHost host)
    {
        _host = host;
        _client = new HttpClient(host.CreateHandler()) { BaseAddress = BaseAddress };
    }

    public CookieContainer Cookies { get; } = new();

    public void Dispose()
    {
        _client.Dispose();
    }

    public bool HasCookie(string name)
    {
        return Cookies.GetCookies(BaseAddress).Any(c => c.Name == name);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, bool csrf = false,
        HttpContent? content = null, string? bearer = null, IDictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        string cookieHeader = Cookies.GetCookieHeader(new Uri(BaseAddress, path));
        if (cookieHeader.Length > 0)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        if (csrf)
        {
            request.Headers.Add(BffRoutes.CsrfHeaderName, BffRoutes.CsrfHeaderValue);
        }

        if (bearer is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }

        foreach (KeyValuePair<string, string> header in headers ?? new Dictionary<string, string>())
        {
            request.Headers.Add(header.Key, header.Value);
        }

        HttpResponseMessage response = await _client.SendAsync(request);
        if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookies))
        {
            foreach (string setCookie in setCookies)
            {
                Cookies.SetCookies(BaseAddress, setCookie);
            }
        }

        return response;
    }

    public Task<HttpResponseMessage> GetAsync(string path, bool csrf = true)
    {
        return SendAsync(HttpMethod.Get, path, csrf);
    }

    ///bff/login → sso-ს authorize (PKCE) → /signin-oidc; აბრუნებს ბოლო პასუხს (გადამისამართება returnUrl-ზე)
    public async Task<HttpResponseMessage> SignInAsync(SsoTestUser user, string returnUrl = "/")
    {
        using HttpResponseMessage challenge =
            await SendAsync(HttpMethod.Get, "/bff/login?returnUrl=" + Uri.EscapeDataString(returnUrl));
        if (challenge.StatusCode != HttpStatusCode.Redirect)
        {
            throw new InvalidOperationException($"/bff/login answered {challenge.StatusCode}");
        }

        Uri authorize = challenge.Headers.Location!;
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(authorize.Query);
        string redirectUri = query["redirect_uri"].ToString();
        string code = _host.Sso.IssueCode(user, query["nonce"].ToString(), query["code_challenge"].ToString(),
            redirectUri);
        var callback = new Uri(redirectUri);
        return await SendAsync(HttpMethod.Get,
            callback.AbsolutePath + "?code=" + code + "&state=" + Uri.EscapeDataString(query["state"].ToString()));
    }
}
