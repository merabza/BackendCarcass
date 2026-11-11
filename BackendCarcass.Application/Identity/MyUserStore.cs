using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Domain.ManyToManyJoins;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendCarcass.Application.Identity;

public sealed class MyUserStore : IUserPasswordStore<AppUser>, IUserEmailStore<AppUser>, IUserRoleStore<AppUser>,
    IUserLockoutStore<AppUser>, IQueryableUserStore<AppUser>, IQueryableRoleStore<AppRole>
{
    private readonly ILogger<MyUserStore> _logger;

    private readonly IIdentityRepository _repo;

    public MyUserStore(IIdentityRepository repo, ILogger<MyUserStore> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
    {
        return await _repo.CreateRoleAsync(role, cancellationToken);
    }

    public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
    {
        return await _repo.RemoveRoleAsync(role.Id, cancellationToken);
    }

    public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
    {
        return await _repo.UpdateRoleAsync(role.Id, role, cancellationToken);
    }

    public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id.ToString(CultureInfo.InvariantCulture));
    }

    public Task<string?> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(AppRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.FromResult(true);
    }

    public Task<string?> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(AppRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.FromResult(true);
    }

    public IQueryable<AppRole> Roles =>
        _repo.Roles.Select(s => new AppRole(s.RolKey, s.RolName, s.RolLevel)
        {
            NormalizedName = s.RolNormalizedKey, Id = s.RolId
        });

    async Task<AppRole?> IRoleStore<AppRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        //იდენტიფიკატორი ბაზაში რიცხვია: ToString-ი SQL-ში ვერ ითარგმნება, ამიტომ სტრიქონი ჯერ რიცხვად გადაიყვანება
        if (!int.TryParse(roleId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
        {
            return null;
        }

        Role? role = await _repo.Roles.FirstOrDefaultAsync(u => u.RolId == id, cancellationToken);
        return role == null
            ? null // throw new Exception($"Role with by roleId={roleId} does not exists")
            : new AppRole(role.RolKey, role.RolName, role.RolLevel)
            {
                NormalizedName = role.RolNormalizedKey, Id = role.RolId
            };
    }

    async Task<AppRole?> IRoleStore<AppRole>.FindByNameAsync(string normalizedRoleName,
        CancellationToken cancellationToken)
    {
        Role? role = await _repo.Roles.FirstOrDefaultAsync(u => u.RolNormalizedKey == normalizedRoleName,
            cancellationToken);
        return role == null
            ? null //throw new Exception($"Role with by normalizedRoleName={normalizedRoleName} does not exists")
            : new AppRole(role.RolKey, role.RolName, role.RolLevel)
            {
                NormalizedName = role.RolNormalizedKey, Id = role.RolId
            };
    }

    public IQueryable<AppUser> Users
    {
        get
        {
            IQueryable<AppUser> ret = _repo.Users.Select(s => new AppUser(s.UserName, s.FirstName, s.LastName)
            {
                Id = s.UsrId,
                PasswordHash = s.PasswordHash,
                NormalizedUserName = s.NormalizedUserName,
                Email = s.Email,
                NormalizedEmail = s.NormalizedEmail,
                EmailConfirmed = s.EmailConfirmed,
                AccessFailedCount = s.AccessFailedCount,
                LockoutEnd = s.LockoutEnd
            });
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("User count: {UserCount}", ret.Count().ToString(CultureInfo.InvariantCulture));
            }

            return ret;
        }
    }

    public Task SetEmailAsync(AppUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.FromResult(true);
    }

    public Task<string?> GetEmailAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public async Task<AppUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        User? user =
            await _repo.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        return user is null ? null : ToAppUser(user);
    }

    public Task<string?> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(AppUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.FromResult(true);
    }

    public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
    {
        return await _repo.CreateUserAsync(user, cancellationToken);
    }

    public Task<IdentityResult> DeleteAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        return _repo.RemoveUserAsync(appUser.Id, cancellationToken);
    }

    public async Task<AppUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        //იდენტიფიკატორი ბაზაში რიცხვია: ToString-ი SQL-ში ვერ ითარგმნება, ამიტომ სტრიქონი ჯერ რიცხვად გადაიყვანება
        if (!int.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
        {
            return null;
        }

        User? user = await _repo.Users.FirstOrDefaultAsync(u => u.UsrId == id, cancellationToken);
        return user is null ? null : ToAppUser(user);
    }

    public async Task<AppUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        //ThrowIfDisposed();

        //return await _repo.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);

        User? user = await _repo.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName,
            cancellationToken);
        return user == null ? null : ToAppUser(user);
    }

    public Task<string?> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString(CultureInfo.InvariantCulture));
    }

    public Task<string?> GetUserNameAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(AppUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.FromResult(true);
    }

    public Task SetUserNameAsync(AppUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.FromResult(true);
    }

    public async Task<IdentityResult> UpdateAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        User? user = await _repo.Users.FirstOrDefaultAsync(u => u.UsrId == appUser.Id, cancellationToken);
        if (user == null || string.IsNullOrWhiteSpace(appUser.UserName) ||
            string.IsNullOrWhiteSpace(appUser.NormalizedUserName) || string.IsNullOrWhiteSpace(appUser.Email) ||
            string.IsNullOrWhiteSpace(appUser.NormalizedEmail) || string.IsNullOrWhiteSpace(appUser.PasswordHash))
        {
            return IdentityResult.Failed();
        }

        //სახელის ან გვარის შეცვლისას სრული სახელიც იცვლება. სხვა ველების განახლება მას არ ეხება: ძველი ბაზებიდან
        //გადმოტანილი სრული სახელი სახელსა და გვარს ყოველთვის არ ემთხვევა (ისინი შეიძლება ცარიელიც იყოს)
        if (user.FirstName != appUser.FirstName || user.LastName != appUser.LastName)
        {
            user.FullName = appUser.FirstName + " " + appUser.LastName;
        }

        user.FirstName = appUser.FirstName;
        user.LastName = appUser.LastName;
        user.UserName = appUser.UserName;
        user.NormalizedUserName = appUser.NormalizedUserName;
        user.Email = appUser.Email;
        user.NormalizedEmail = appUser.NormalizedEmail;
        user.PasswordHash = appUser.PasswordHash;
        user.EmailConfirmed = appUser.EmailConfirmed;
        user.AccessFailedCount = appUser.AccessFailedCount;
        user.LockoutEnd = appUser.LockoutEnd;
        return await _repo.UpdateUserAsync(user.UsrId, user, cancellationToken);
    }

    public Task SetPasswordHashAsync(AppUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.FromResult(true);
    }

    public async Task<string?> GetPasswordHashAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        User? user = await _repo.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == appUser.NormalizedUserName,
            cancellationToken);
        return
            user?.PasswordHash; // ??throw new Exception($"GetPasswordHashAsync cannot find AppUser NormalizedUserName={appUser.NormalizedUserName}");
    }

    public async Task<bool> HasPasswordAsync(AppUser appUser, CancellationToken cancellationToken)
    {
        User? user = await _repo.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == appUser.NormalizedUserName,
            cancellationToken);
        return !string.IsNullOrWhiteSpace(user?.PasswordHash);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.AccessFailedCount);
    }

    //ბლოკირება ყველა მომხმარებლისთვის ჩართულია, ამიტომ ცალკე სვეტი არ გვჭირდება
    public Task<bool> GetLockoutEnabledAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task SetLockoutEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    private static AppUser ToAppUser(User user)
    {
        return new AppUser(user.UserName, user.FirstName, user.LastName)
        {
            Id = user.UsrId,
            PasswordHash = user.PasswordHash,
            NormalizedUserName = user.NormalizedUserName,
            Email = user.Email,
            NormalizedEmail = user.NormalizedEmail,
            EmailConfirmed = user.EmailConfirmed,
            AccessFailedCount = user.AccessFailedCount,
            LockoutEnd = user.LockoutEnd
        };
    }

    public async Task AddToRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
    {
        Role? role = await _repo.Roles.FirstOrDefaultAsync(r => r.RolKey == roleName, cancellationToken);
        if (role == null)
        {
            return;
        }

        await _repo.UserAddToRoleAsync(user.Id, role.RolId);
    }

    public async Task RemoveFromRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
    {
        Role? role = await _repo.Roles.FirstOrDefaultAsync(r => r.RolKey == roleName, cancellationToken);
        if (role == null)
        {
            return;
        }

        ManyToManyJoin? match = await _repo.RolesByUsers.FirstOrDefaultAsync(
            ru => ru.PKey == user.UserName && ru.CKey == role.RolKey, cancellationToken);
        if (match == null)
        {
            return;
        }

        _repo.RemoveUserFromRole(match);
    }

    public async Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken cancellationToken)
    {
        return await _repo.RolesByUsers.Where(ru => ru.PKey == user.UserName).Select(s => s.CKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsInRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
    {
        Role? role = await _repo.Roles.FirstOrDefaultAsync(r => r.RolKey == roleName, cancellationToken);
        if (role == null)
        {
            return false;
        }

        ManyToManyJoin? roleByUser =
            await _repo.RolesByUsers.FirstOrDefaultAsync(ru => ru.PKey == user.UserName && ru.CKey == role.RolKey,
                cancellationToken);
        return roleByUser != null;
    }

    async Task<IList<AppUser>> IUserRoleStore<AppUser>.GetUsersInRoleAsync(string roleName,
        CancellationToken cancellationToken)
    {
        IList<AppUser> us = await _repo.Users.Select(s =>
            new AppUser(s.UserName, s.FirstName, s.LastName)
            {
                Id = s.UsrId, NormalizedUserName = s.NormalizedUserName
            }).ToListAsync(cancellationToken);
        return us;
    }
}
