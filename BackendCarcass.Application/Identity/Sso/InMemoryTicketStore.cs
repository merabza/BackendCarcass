using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BackendCarcass.Application.Identity.Sso;

//აპის სესიები სერვერის მეხსიერებაში (D16): cookie-ში მხოლოდ გასაღებია, ტოკენები აქ რჩება. ინდექსები sid-ითა და
//sub-ით back-channel logout-ს სჭირდება. ჰოსტის რესტარტისას სესიები იკარგება და მომხმარებელი sso-ზე ხელახლა,
//პაროლის გარეშე შედის
public sealed class InMemoryTicketStore : ITicketStore, IDisposable
{
    private const string KeyPrefix = "carcass-sso-ticket:";

    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly TimeSpan _idleTimeout;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _bySid = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _bySub = new(StringComparer.Ordinal);

    public InMemoryTicketStore(IOptions<CarcassAuthenticationSettings> settings)
    {
        _idleTimeout = settings.Value.Sso.SessionIdleTimeout;
    }

    public int Count => _cache.Count;

    public void Dispose()
    {
        _cache.Dispose();
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        string key = Guid.NewGuid().ToString("N");
        Put(key, ticket);
        return Task.FromResult(key);
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        Put(key, ticket);
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        return Task.FromResult(_cache.Get<AuthenticationTicket>(KeyPrefix + key));
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(KeyPrefix + key);
        return Task.CompletedTask;
    }

    //ყველა სესია ამ sso-სესიით (sid); აბრუნებს გაუქმებული სესიების რაოდენობას
    public int RemoveBySid(string sid)
    {
        return RemoveIndexed(_bySid, sid);
    }

    //ყველა სესია ამ მომხმარებლით (sub)
    public int RemoveBySub(string sub)
    {
        return RemoveIndexed(_bySub, sub);
    }

    private int RemoveIndexed(ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> index, string value)
    {
        if (!index.TryRemove(value, out ConcurrentDictionary<string, byte>? keys))
        {
            return 0;
        }

        List<string> existing = [.. keys.Keys.Where(k => _cache.TryGetValue(KeyPrefix + k, out _))];
        foreach (string key in existing)
        {
            _cache.Remove(KeyPrefix + key);
        }

        return existing.Count;
    }

    private void Put(string key, AuthenticationTicket ticket)
    {
        string? sid = ticket.Principal.FindFirstValue(SsoClaimTypes.SessionId);
        string? sub = ticket.Principal.FindFirstValue(SsoClaimTypes.Subject);
        var options = new MemoryCacheEntryOptions { SlidingExpiration = _idleTimeout };
        options.RegisterPostEvictionCallback((_, _, _, _) =>
        {
            Unindex(_bySid, sid, key);
            Unindex(_bySub, sub, key);
        });
        _cache.Set(KeyPrefix + key, ticket, options);
        Index(_bySid, sid, key);
        Index(_bySub, sub, key);
    }

    private static void Index(ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> index, string? value,
        string key)
    {
        if (!string.IsNullOrEmpty(value))
        {
            index.GetOrAdd(value, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal))[key] = 0;
        }
    }

    private static void Unindex(ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> index, string? value,
        string key)
    {
        if (!string.IsNullOrEmpty(value) && index.TryGetValue(value, out ConcurrentDictionary<string, byte>? keys))
        {
            keys.TryRemove(key, out _);
        }
    }
}
