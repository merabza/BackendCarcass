using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Database.Models;
using BackendCarcass.MasterData.Models;
using Microsoft.AspNetCore.Identity;

namespace BackendCarcass.Identity;

public interface IIdentityRepository
{
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<ManyToManyJoin> RolesByUsers { get; }

    ValueTask<IdentityResult> CreateUserAsync(AppUser appUser, CancellationToken cancellationToken = default);
    Task<IdentityResult> RemoveUserAsync(int userId, CancellationToken cancellationToken = default);
    ValueTask<IdentityResult> UpdateUserAsync(int userId, User user, CancellationToken cancellationToken = default);
    ValueTask<IdentityResult> CreateRoleAsync(AppRole appRole, CancellationToken cancellationToken = default);
    ValueTask<IdentityResult> RemoveRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task UserAddToRoleAsync(int userId, int roleRId);
    void RemoveUserFromRole(ManyToManyJoin match);
    ValueTask<IdentityResult> UpdateRoleAsync(int roleId, AppRole role, CancellationToken cancellationToken = default);
}