namespace BackendCarcass.Application.Identity.Sso;

public sealed record BackchannelLogoutSubject(string? SessionId, string? Subject);
