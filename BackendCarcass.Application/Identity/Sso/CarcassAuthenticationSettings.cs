using System;
using System.Collections.Generic;

namespace BackendCarcass.Application.Identity.Sso;

//ავტორიზაციის რეჟიმი: Local — აპის საკუთარი შესვლა და JWT (ნაგულისხმევი), Sso — sso.ltg.ge OpenID Connect-ით (BFF)
public enum EAuthenticationMode
{
    Local,
    Sso
}

//კონფიგურაციის სექცია "Authentication" (sso-design.md, D15–D17)
public sealed class CarcassAuthenticationSettings
{
    public const string SectionName = "Authentication";

    public EAuthenticationMode Mode { get; set; } = EAuthenticationMode.Local;

    //აპის სესიის cookie-ს სახელი: production-ში __Host-errata, dev-ში errata.bff (localhost-ის პორტები cookie-ებს
    //იზიარებს, ამიტომ თითო აპს უნიკალური სახელი აქვს)
    public string? CookieName { get; set; }

    public SsoClientSettings Sso { get; set; } = new();

    //მანქანური კლიენტები: client_id → აპის ლოკალური ტექნიკური მომხმარებლის UserName (D17, D23)
    public Dictionary<string, string> MachineClients { get; set; } = new(StringComparer.Ordinal);
}

public sealed class SsoClientSettings
{
    //sso.ltg.ge-ის მისამართი, მაგალითად https://sso.ltg.ge/ (dev-ში http://localhost:5090/)
    public string? Authority { get; set; }

    public string? ClientId { get; set; }

    //საიდუმლო: მხოლოდ user-secrets-ში ან production-ის კონფიგურაციაში
    public string? ClientSecret { get; set; }

    //აპის API-ს scope და audience, მაგალითად api.errata
    public string? ApiScope { get; set; }

    //დამატებითი scope-ები openid, profile, email და offline_access-ის გარდა
    public List<string> Scopes { get; set; } = [];

    //აპის სესიის უმოქმედობის ლიმიტი (D4)
    public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromHours(8);

    //sso-ს მიუწვდომლობისას ძველი წვდომის ტოკენით მუშაობის ვადა (D24)
    public TimeSpan OutageGracePeriod { get; set; } = TimeSpan.FromMinutes(30);
}
