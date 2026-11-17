using System;
using System.Collections.Generic;

namespace BackendCarcass.Application.Identity.Sso;

//ავტორიზაციის რეჟიმი: Local — აპის საკუთარი შესვლა და JWT (ნაგულისხმევი), Sso — sso.ltg.ge OpenID Connect-ით (BFF)

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
