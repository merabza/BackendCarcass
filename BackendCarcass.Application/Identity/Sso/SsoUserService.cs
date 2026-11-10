using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.Identity.Sso;

//sso.ltg.ge-ის მომხმარებლის დაკავშირება აპის ლოკალურ მომხმარებელთან (sso-design.md, D12–D13, D16–D17).
//აპის სესიის principal-ი ისეთივე claim-ებს შეიცავს, როგორსაც CurrentUser და RightsDeterminer ელის
public sealed class SsoUserService
{
    private readonly ICarcassApplicationDbContext _db;
    private readonly ILogger<SsoUserService> _logger;

    public SsoUserService(ICarcassApplicationDbContext db, ILogger<SsoUserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    //შესვლა: მომხმარებლის პოვნა sub-ით, ან ბმა დადასტურებული ელფოსტით, ან შექმნა როლების გარეშე (JIT);
    //შემდეგ ელფოსტის, სახელების და UserName-ის სინქრონიზაცია. ყველაფერი ერთ ტრანზაქციაშია
    public async Task<Result<ClaimsPrincipal>> SignInAsync(ClaimsPrincipal external, string authenticationType,
        CancellationToken cancellationToken = default)
    {
        SsoProfile? profile = SsoProfile.From(external);
        if (profile is null)
        {
            return Result.Failure<ClaimsPrincipal>(string.IsNullOrWhiteSpace(external.FindFirstValue(SsoClaimTypes.Subject))
                ? SsoErrors.SubjectIsMissing
                : SsoErrors.UserNameIsMissing);
        }

        await using IDbContextTransaction transaction = await _db.BeginTransactionAsync(cancellationToken);

        Result<User> user = await FindOrCreateAsync(profile, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure<ClaimsPrincipal>(user.Error);
        }

        await SyncAsync(user.Value, profile, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await CreatePrincipalAsync(user.Value, profile.Subject, profile.SessionId, authenticationType,
            cancellationToken);
    }

    //ტოკენის განახლებისას: მომხმარებელი და მისი როლები ბაზიდან თავიდან იკითხება (D16)
    public async Task<Result<ClaimsPrincipal>> RefreshAsync(ClaimsPrincipal current, string authenticationType,
        CancellationToken cancellationToken = default)
    {
        string? subject = current.FindFirstValue(SsoClaimTypes.Subject);
        if (!int.TryParse(current.FindFirstValue(ClaimTypes.Sid), NumberStyles.None, CultureInfo.InvariantCulture,
                out int userId) || subject is null)
        {
            return Result.Failure<ClaimsPrincipal>(SsoErrors.SubjectIsMissing);
        }

        User? user = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.UsrId == userId && u.SsoSubject == subject, cancellationToken);
        if (user is null)
        {
            return Result.Failure<ClaimsPrincipal>(SsoErrors.SubjectIsMissing);
        }

        return await CreatePrincipalAsync(user, subject, current.FindFirstValue(SsoClaimTypes.SessionId),
            authenticationType, cancellationToken);
    }

    //მანქანური კლიენტი (client_credentials): ლოკალური ტექნიკური მომხმარებელი კონფიგურაციიდან (D17)
    public async Task<Result<ClaimsPrincipal>> MachineSignInAsync(string clientId, string localUserName,
        string authenticationType, CancellationToken cancellationToken = default)
    {
        string normalizedUserName = Normalize(localUserName);
        User? user = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
        if (user is null)
        {
            _logger.LogError("The technical user of the machine client {ClientId} does not exist", clientId);
            return Result.Failure<ClaimsPrincipal>(SsoErrors.UnknownMachineClient);
        }

        Result<ClaimsPrincipal> principal =
            await CreatePrincipalAsync(user, clientId, null, authenticationType, cancellationToken);
        ((ClaimsIdentity)principal.Value.Identity!).AddClaim(new Claim(SsoClaimTypes.ClientId, clientId));
        return principal;
    }

    public Task<User?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UsrId == userId, cancellationToken);
    }

    private async Task<Result<User>> FindOrCreateAsync(SsoProfile profile, CancellationToken cancellationToken)
    {
        User? user = await _db.Users.SingleOrDefaultAsync(u => u.SsoSubject == profile.Subject, cancellationToken);
        if (user is not null)
        {
            return user;
        }

        string normalizedEmail = Normalize(profile.Email);
        if (profile.EmailVerified && normalizedEmail.Length > 0)
        {
            user = await _db.Users.SingleOrDefaultAsync(
                u => u.NormalizedEmail == normalizedEmail && u.SsoSubject == null, cancellationToken);
            if (user is not null)
            {
                user.SsoSubject = profile.Subject;
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Local user {UserId} linked to the sso account by e-mail", user.UsrId);
                }

                return user;
            }
        }

        //ელფოსტა ლოკალურად სხვა ანგარიშს ეკუთვნის: ეს მონაცემების კონფლიქტია ადმინისტრატორისთვის (D12.4)
        if (normalizedEmail.Length > 0 &&
            await _db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            _logger.LogError("Sso sign-in refused: the e-mail belongs to another local user (subject {Subject})",
                profile.Subject);
            return Result.Failure<User>(SsoErrors.EmailLinkedToAnotherAccount);
        }

        string userName = await FreeUserNameAsync(profile.UserName, cancellationToken);
        user = new User
        {
            UserName = userName,
            NormalizedUserName = Normalize(userName),
            Email = profile.Email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = CarcassSsoDefaults.UnusableHashMarker,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            FullName = profile.FullName,
            EmailConfirmed = true,
            SsoSubject = profile.Subject
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Local user {UserId} created for the sso account (no roles)", user.UsrId);
        }

        return user;
    }

    //UserName-ის ლოკალური კონფლიქტისას სუფიქსი -2, -3, ... (პირველი თავისუფალი)
    private async Task<string> FreeUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        string candidate = userName;
        int suffix = 2;
        while (await UserNameTakenAsync(candidate, null, cancellationToken))
        {
            candidate = string.Create(CultureInfo.InvariantCulture, $"{userName}-{suffix}");
            suffix++;
        }

        if (candidate != userName)
        {
            _logger.LogWarning("Sso user name is taken locally, the local user gets {UserName}", candidate);
        }

        return candidate;
    }

    private Task<bool> UserNameTakenAsync(string userName, int? exceptUserId, CancellationToken cancellationToken)
    {
        string normalized = Normalize(userName);
        return _db.Users.AnyAsync(u => u.NormalizedUserName == normalized && u.UsrId != exceptUserId,
            cancellationToken);
    }

    private async Task SyncAsync(User user, SsoProfile profile, CancellationToken cancellationToken)
    {
        user.FirstName = profile.FirstName;
        user.LastName = profile.LastName;
        user.FullName = profile.FullName;

        string normalizedEmail = Normalize(profile.Email);
        if (normalizedEmail.Length > 0 && user.Email != profile.Email)
        {
            if (await _db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.UsrId != user.UsrId,
                    cancellationToken))
            {
                _logger.LogWarning("E-mail of local user {UserId} not synced: taken by another user", user.UsrId);
            }
            else
            {
                user.Email = profile.Email;
                user.NormalizedEmail = normalizedEmail;
            }
        }

        if (user.UserName == profile.UserName)
        {
            return;
        }

        if (await UserNameTakenAsync(profile.UserName, user.UsrId, cancellationToken))
        {
            _logger.LogWarning("User name of local user {UserId} not synced: taken by another user", user.UsrId);
            return;
        }

        await RenameInJoinsAsync(user, profile.UserName, cancellationToken);
        user.UserName = profile.UserName;
        user.NormalizedUserName = Normalize(profile.UserName);
    }

    //ManyToManyJoins-ში მომხმარებელი UserName-ით ან NormalizedUserName-ით არის მითითებული (D13)
    private async Task RenameInJoinsAsync(User user, string newUserName, CancellationToken cancellationToken)
    {
        string usersTable = _db.GetTableName<User>();
        int userDataTypeId = await _db.DataTypes.Where(d => d.DtTable == usersTable).Select(d => d.DtId)
            .SingleOrDefaultAsync(cancellationToken);
        if (userDataTypeId == 0)
        {
            return;
        }

        string[] oldKeys = [user.UserName, user.NormalizedUserName];
        await _db.ManyToManyJoins.Where(m => m.PtId == userDataTypeId && oldKeys.Contains(m.PKey))
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.PKey, newUserName), cancellationToken);
        await _db.ManyToManyJoins.Where(m => m.CtId == userDataTypeId && oldKeys.Contains(m.CKey))
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.CKey, newUserName), cancellationToken);
    }

    private async Task<Result<ClaimsPrincipal>> CreatePrincipalAsync(User user, string subject, string? sessionId,
        string authenticationType, CancellationToken cancellationToken)
    {
        List<string> roles = await GetRolesAsync(user.UserName, cancellationToken);
        List<Claim> claims =
        [
            new(SsoClaimTypes.Subject, subject),
            new(ClaimTypes.Sid, user.UsrId.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.SerialNumber, "0"),
            .. roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];
        if (!string.IsNullOrEmpty(sessionId))
        {
            claims.Add(new Claim(SsoClaimTypes.SessionId, sessionId));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType, ClaimTypes.Name, ClaimTypes.Role));
    }

    //User→Role ბმა ManyToManyJoins-შია UserName-ის მიხედვით, როგორც MyUserStore.GetRolesAsync-ში
    private Task<List<string>> GetRolesAsync(string userName, CancellationToken cancellationToken)
    {
        string usersTable = _db.GetTableName<User>();
        string rolesTable = _db.GetTableName<Role>();
        return _db.ManyToManyJoins
            .Where(m => m.ParentDataTypeNavigation.DtTable == usersTable &&
                        m.ChildDataTypeNavigation.DtTable == rolesTable && m.PKey == userName)
            .Select(m => m.CKey).ToListAsync(cancellationToken);
    }

    private static string Normalize(string value)
    {
        return value.ToUpperInvariant();
    }

    //id_token-ის claim-ები (D9)
    private sealed record SsoProfile(string Subject, string? SessionId, string UserName, string Email,
        bool EmailVerified, string FirstName, string LastName, string FullName)
    {
        public static SsoProfile? From(ClaimsPrincipal principal)
        {
            string? subject = principal.FindFirstValue(SsoClaimTypes.Subject);
            string? userName = principal.FindFirstValue(SsoClaimTypes.PreferredUserName);
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            string firstName = principal.FindFirstValue(SsoClaimTypes.GivenName) ?? string.Empty;
            string lastName = principal.FindFirstValue(SsoClaimTypes.FamilyName) ?? string.Empty;
            string? name = principal.FindFirstValue(SsoClaimTypes.Name);
            return new SsoProfile(subject, principal.FindFirstValue(SsoClaimTypes.SessionId), userName,
                principal.FindFirstValue(SsoClaimTypes.Email) ?? string.Empty,
                string.Equals(principal.FindFirstValue(SsoClaimTypes.EmailVerified), "true",
                    StringComparison.OrdinalIgnoreCase), firstName, lastName,
                string.IsNullOrWhiteSpace(name) ? $"{firstName} {lastName}".Trim() : name);
        }
    }
}
