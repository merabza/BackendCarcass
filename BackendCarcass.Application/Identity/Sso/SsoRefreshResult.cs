namespace BackendCarcass.Application.Identity.Sso;

public sealed record SsoRefreshResult(
    ESsoRefreshOutcome Outcome,
    string? AccessToken = null,
    string? RefreshToken = null,
    string? IdToken = null,
    int ExpiresIn = 0)
{
    public static SsoRefreshResult Rejected { get; } = new(ESsoRefreshOutcome.Rejected);
    public static SsoRefreshResult Unavailable { get; } = new(ESsoRefreshOutcome.Unavailable);
}
