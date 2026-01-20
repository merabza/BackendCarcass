using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Database;
using BackendCarcass.Database.Models;
using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.Identity;

public sealed class IdentityRepository : IIdentityRepository
{
    private readonly CarcassDbContext _carcassContext;

    //private readonly IDataTypeKeys _dataTypeKeys;
    private readonly ILogger<IdentityRepository> _logger;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public IdentityRepository(CarcassDbContext ctx, IUnitOfWork unitOfWork, ILogger<IdentityRepository> logger)
    {
        _carcassContext = ctx;
        _logger = logger;
        _unitOfWork = unitOfWork;
        //_dataTypeKeys = dataTypeKeys;
    }

    public IQueryable<User> Users => _carcassContext.Users;
    public IQueryable<Role> Roles => _carcassContext.Roles;

    public IQueryable<ManyToManyJoin> RolesByUsers =>
        _carcassContext.ManyToManyJoins.Include(i => i.ParentDataTypeNavigation).Include(i => i.ChildDataTypeNavigation)
            .Where(w => w.ParentDataTypeNavigation.DtTable == _unitOfWork.GetTableName<User>() &&
                        w.ChildDataTypeNavigation.DtTable == _unitOfWork.GetTableName<Role>());

    public async Task<IdentityResult> RemoveUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldUser = await _carcassContext.Users.SingleOrDefaultAsync(u => u.UsrId == userId, cancellationToken);
            if (oldUser == null)
            {
                return IdentityResult.Failed();
            }

            _carcassContext.Remove(oldUser);
            await _carcassContext.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while removing a user.");
            return IdentityResult.Failed();
        }
    }

    public async ValueTask<IdentityResult> UpdateUserAsync(int userId, User user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var oldUser = await _carcassContext.Users.SingleOrDefaultAsync(r => r.UsrId == userId, cancellationToken);
            if (oldUser == null)
            {
                return IdentityResult.Failed();
            }

            oldUser.UserName = user.UserName;
            oldUser.NormalizedUserName = user.NormalizedUserName;
            oldUser.Email = user.Email;
            oldUser.NormalizedEmail = user.NormalizedEmail;
            oldUser.PasswordHash = user.PasswordHash;
            oldUser.FirstName = user.FirstName;
            oldUser.LastName = user.LastName;
            oldUser.FullName = user.FullName;
            _carcassContext.Update(oldUser);
            await _carcassContext.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while updating a user.");
            return IdentityResult.Failed();
        }
    }

    public async ValueTask<IdentityResult> RemoveRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldRole = await _carcassContext.Roles.SingleOrDefaultAsync(r => r.RolId == roleId, cancellationToken);
            if (oldRole == null)
            {
                return IdentityResult.Failed();
            }

            _carcassContext.Remove(oldRole);
            await _carcassContext.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while removing a role.");
            return IdentityResult.Failed();
        }
    }

    public Task UserAddToRoleAsync(int userId, int roleRId)
    {
        throw new NotImplementedException();
    }

    public void RemoveUserFromRole(ManyToManyJoin match)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IdentityResult> CreateUserAsync(AppUser appUser,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(appUser.UserName) || string.IsNullOrWhiteSpace(appUser.NormalizedUserName) ||
            string.IsNullOrWhiteSpace(appUser.Email) || string.IsNullOrWhiteSpace(appUser.NormalizedEmail) ||
            string.IsNullOrWhiteSpace(appUser.PasswordHash))
        {
            _logger.Log(LogLevel.Error, "Invalid appUser");
            return IdentityResult.Failed();
        }

        try
        {
            var user = new User
            {
                UserName = appUser.UserName,
                NormalizedUserName = appUser.NormalizedUserName,
                Email = appUser.Email,
                NormalizedEmail = appUser.NormalizedEmail,
                PasswordHash = appUser.PasswordHash,
                FullName = appUser.FullName,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName
            };
            _carcassContext.Users.Add(user);
            await _carcassContext.SaveChangesAsync(cancellationToken);
            appUser.Id = user.UsrId;
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while creating a user.");
            return IdentityResult.Failed();
        }
    }

    public async ValueTask<IdentityResult> CreateRoleAsync(AppRole appRole,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(appRole.Name) || string.IsNullOrWhiteSpace(appRole.NormalizedName))
        {
            _logger.Log(LogLevel.Error, "Invalid appRole");
            return IdentityResult.Failed();
        }

        try
        {
            var role = new Role
            {
                RolKey = appRole.Name,
                RolName = appRole.RoleName,
                RolLevel = appRole.Level,
                RolNormalizedKey = appRole.NormalizedName
            };
            _carcassContext.Roles.Add(role);
            await _carcassContext.SaveChangesAsync(cancellationToken);
            appRole.Id = role.RolId;
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while creating a role.");
            return IdentityResult.Failed();
        }
    }

    public async ValueTask<IdentityResult> UpdateRoleAsync(int roleId, AppRole role,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(role.Name) || string.IsNullOrWhiteSpace(role.NormalizedName))
        {
            _logger.Log(LogLevel.Error, "Invalid appRole");
            return IdentityResult.Failed();
        }

        try
        {
            var oldRole = await _carcassContext.Roles.SingleOrDefaultAsync(r => r.RolId == roleId, cancellationToken);
            if (oldRole == null)
            {
                return IdentityResult.Failed();
            }

            oldRole.RolKey = role.Name;
            oldRole.RolName = role.RoleName;
            oldRole.RolLevel = role.Level;
            oldRole.RolNormalizedKey = role.NormalizedName;
            _carcassContext.Update(oldRole);
            await _carcassContext.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while updating a role.");
            return IdentityResult.Failed();
        }
    }
}
