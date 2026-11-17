using System;
using System.Collections.Generic;

namespace BackendCarcass.Application.Identity.Sso;

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
