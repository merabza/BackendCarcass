namespace BackendCarcass.Application.Identity.Sso;

//OpenID Connect-ის claim-ები, რომლებსაც SSO რეჟიმი იყენებს (sso-design.md, D9)
public static class SsoClaimTypes
{
    public const string Subject = "sub";
    public const string SessionId = "sid";
    public const string PreferredUserName = "preferred_username";
    public const string Email = "email";
    public const string EmailVerified = "email_verified";
    public const string GivenName = "given_name";
    public const string FamilyName = "family_name";
    public const string Name = "name";
    public const string ClientId = "client_id";
}
