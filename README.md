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

## SSO mode

With `Authentication:Mode = Sso` (default `Local`) the app host signs users in through sso.ltg.ge with OpenID Connect
as a confidential client and a backend for frontend (merabza/SsoLtgGe#6, design `docs/sso-design.md` D11–D18 in
merabza/SsoLtgGe). The browser gets only an HttpOnly session cookie; the tokens stay in server memory. Local mode
registers exactly what it did before.

The app calls the same three methods as in Local mode; they read the mode themselves:

| Method | Sso mode |
|---|---|
| `AddCarcassIdentity` | Identity core (unchanged), then cookie + OpenID Connect (code + PKCE + client secret) + JWT bearer for machine clients behind the policy scheme `CarcassSmart`, the in-memory ticket store, antiforgery; no app JWT |
| `UseAuthenticationAndAuthorization` | adds the CSRF check before authentication |
| `UseBackendCarcassApiEndpoints` | maps `/bff/*` instead of `/api/v1/authentication/*`; leaves out `userrights/changeprofile`, `changepassword` and `deletecurrentuser` (the account lives on sso) |

Configuration (secrets only in user secrets or the production configuration):

```json
"Authentication": {
  "Mode": "Sso",
  "CookieName": "errata.bff",
  "Sso": {
    "Authority": "http://localhost:5090/",
    "ClientId": "app-errata",
    "ClientSecret": "<user-secrets>",
    "ApiScope": "api.errata"
  },
  "MachineClients": { "grammar-worker": "<local technical UserName>" }
}
```

- `CookieName`: production `__Host-errata` / `__Host-ganmarteba` / `__Host-grammar` (Secure is then forced), dev
  `errata.bff` / `ganmarteba.bff` / `grammar.bff` (localhost ports share cookies). The correlation, nonce and
  antiforgery cookies get the same prefix.
- Optional `Sso:Scopes` (extra scopes), `Sso:SessionIdleTimeout` (default 8 h), `Sso:OutageGracePeriod` (default
  30 min). HTTPS metadata is required unless the authority is a loopback address.
- Behind nginx the host must see the original scheme and host (forwarded headers), or the redirect URI is wrong.

Endpoints:

| Endpoint | Purpose |
|---|---|
| `GET /bff/login?returnUrl=` | redirect to sso; only a local `returnUrl` is kept, otherwise `/` |
| `POST /bff/logout` | form with the antiforgery token from `/bff/user`; revokes the refresh token, ends the session, redirects to sso end-session (which logs out the other apps) |
| `GET /bff/user` | `BffUserResponse`: user, roles, app claims and the logout form token; 401 without a session |
| `POST /bff/backchannel` | back-channel logout receiver (`logout_token`: signature by JWKS, `typ`, `iss`, `aud`, `iat`/`exp`, `events`, no `nonce`, `jti` replay); ends the sessions of the `sid`, or of the `sub` |
| `/signin-oidc`, `/signout-callback-oidc` | handler callbacks |

- Every `/api/*` and `/bff/*` request with the session cookie needs the header `X-CSRF: 1` (401 otherwise);
  WebSocket upgrades are checked by `Origin` instead. Without a session the API answers 401, never a redirect.
- `Authorization: Bearer` is accepted only for a client_credentials token of a client listed in `MachineClients`
  (`sub` = `client_id`, audience `ApiScope`); it acts as that local user with its database roles.
- Users: the local user is found by `Users.SsoSubject`, else linked by a verified e-mail, else created without roles
  (password hash `!sso`, never valid locally). At every sign-in e-mail, names and UserName are synced; a UserName
  change updates `ManyToManyJoins` (`PKey`/`CKey`, both forms) in the same transaction. Roles are re-read from the
  database at each token refresh (access token 10 min).
- Sessions are in memory: a host restart ends them, and the next sign-in at sso needs no password.

### Database: `Users.SsoSubject`

`User` has a new nullable column `SsoSubject` nvarchar(64) with the unique filtered index `IX_Users_SsoSubject`
(empty in Local mode). **Every Carcass database needs it before an app on this version starts, in either mode**,
because EF reads the column with the users:

- dev databases: Recreate Dev Database in the app's DbTools;
- production: the idempotent script [`docs/sso-column.sql`](docs/sso-column.sql) after a backup (merabza/SsoLtgGe#20).

### Connecting an app

1. Add the column (above) and register the client on sso (`Sso:Clients` of SsoLtgGe).
2. Set `Authentication` in the app configuration and the client secret in user secrets.
3. In dev, proxy `/api` (with `ws: true`), `/bff`, `/signin-oidc` and `/signout-callback-oidc` from Vite to the host.
4. Frontend: appcarcass in Sso mode (merabza/SsoLtgGe#7).
