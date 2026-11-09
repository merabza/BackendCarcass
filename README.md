# BackendCarcass
Backend Carcass Projects

## Authentication

`AddCarcassIdentity(debugLogger, configuration)` configures everything an app needs: ASP.NET Core Identity on
Carcass' own store with the password policy and lockout, JWT bearer authentication, e-mail sending and the
`CarcassAuthentication` rate limiter. Its parts can also be called alone, e.g. by a host that signs users in without
the app JWT (sso.ltg.ge, merabza/SsoLtgGe#5):

| Method | Registers |
|---|---|
| `AddCarcassIdentityCore(configuration)` | Identity, password policy, lockout, stores, `IdentitySettings`, default `ILoginTokenIssuer` and `IPendingLoginStore` |
| `AddCarcassJwtAuthentication(configuration)` | JWT bearer as the default scheme (`JwtSecret`, `JwtIssuer`, `JwtAudience`) |
| `AddCarcassEmailSending(configuration)` | `EmailSettings` and `IEmailSender` (SMTP, or the log in Development) |
| `AddCarcassAuthenticationRateLimiter()` | the `CarcassAuthentication` rate-limit policy |

The sign-in steps (user, password, lockout, `NextStep` ConfirmEmail / ChangePassword / None) are decided by
`LoginService`, `LoginCompletionService` and `PendingLoginService`; what the user finally gets is pluggable:

- `ILoginTokenIssuer`: the full login. Default `JwtLoginTokenIssuer`: access token and refresh token, as before.
- `IPendingLoginStore`: the state of an unfinished step. Default `JwtPendingLoginStore`: the pending JWT (audience
  `<JwtAudience>/pending`) that goes to the client as `LoginResponse.Token` and comes back with the next step.
- `ILoginCheck` (none by default, any number): an extra check after a correct password and before a full login, e.g.
  refusing a deactivated user; its error, with its Georgian message, goes to the user unchanged.

The defaults are registered with `TryAdd`, so a host registers its own implementations before calling the core.

New `IdentitySettings` keys (the defaults keep the previous behaviour):

| Key | Default | Meaning |
|---|---|---|
| `AllowEmailSignIn` | `false` | sign in by user name or e-mail: an input with `@` is looked up by e-mail first, otherwise by name, then the other way |
| `ConfirmEmailPath` | `/confirm-email` | path of the e-mail confirmation link after `FrontendBaseUrl` |
| `ResetPasswordPath` | `/reset-password` | path of the password reset link after `FrontendBaseUrl` |

`EEmailVerificationPurpose` has `EmailChange = 3` (code only, no link).

Compatibility: the public methods of the authentication services and of `AddCarcassIdentity` are unchanged; only
the constructors of `LoginService`, `LoginCompletionService` and `PendingLoginService` changed,
which only dependency injection calls.
