namespace BackendCarcass.Application.Identity.Sso;

//SSO რეჟიმის ავტორიზაციის სქემების სახელები
public static class CarcassSsoDefaults
{
    //აპის სესიის cookie (ბრაუზერი)
    public const string CookieScheme = "CarcassSsoCookie";

    //OpenID Connect sso.ltg.ge-სთან
    public const string OpenIdConnectScheme = "CarcassSso";

    //ირჩევს cookie-ს ან Bearer-ს (მანქანური კლიენტი) Authorization header-ის მიხედვით
    public const string PolicyScheme = "CarcassSmart";

    public const string CallbackPath = "/signin-oidc";
    public const string SignedOutCallbackPath = "/signout-callback-oidc";

    //გამოუსადეგარი პაროლის ნიშანი JIT-ით შექმნილი მომხმარებლისთვის: v3 hash არასოდეს არ არის, ამიტომ Local
    //რეჟიმში ამ პაროლით შესვლა შეუძლებელია (D12)
    public const string UnusableHashMarker = "!sso";
}
