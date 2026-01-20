using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BackendCarcass.Identity;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CurrentUser(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public int Id => GetClaimValue<int>(ClaimTypes.Sid);
    public int SerialNumber => GetClaimValue<int>(ClaimTypes.SerialNumber);
    public string Name => GetClaimValue<string>(ClaimTypes.Name);
    public List<string> Roles => GetClaimValues(ClaimTypes.Role);

    private T GetClaimValue<T>(string type) where T : IConvertible
    {
        string? value = _httpContext.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == type)?.Value;

        return value != null
            ? (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture)
            : throw new UnauthorizedAccessException($"{type} claim not found");
    }

    private List<string> GetClaimValues(string type)
    {
        return _httpContext.HttpContext is null
            ? []
            : _httpContext.HttpContext.User.Claims.Where(so => so.Type == type).Select(claim => claim.Value).ToList();
    }
}
