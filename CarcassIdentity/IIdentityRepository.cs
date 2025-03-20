using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace CarcassIdentity;

public interface IIdentityRepository
{
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<ManyToManyJoin> RolesByUsers { get; }

    ValueTask<IdentityResult> CreateUserAsync(AppUser user, CancellationToken cancellationToken = default);
    Task<IdentityResult> RemoveUserAsync(int userId, CancellationToken cancellationToken = default);
    ValueTask<IdentityResult> UpdateUserAsync(int userId, User user, CancellationToken cancellationToken = default);
    ValueTask<IdentityResult> CreateRoleAsync(AppRole role, CancellationToken cancellationToken = default);
    ValueTask<IdentityResult> RemoveRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task UserAddToRoleAsync(int userId, int roleRId);
    void RemoveUserFromRole(ManyToManyJoin match);
    ValueTask<IdentityResult> UpdateRoleAsync(int roleId, AppRole role, CancellationToken cancellationToken = default);
}