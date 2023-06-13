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


    Task<IdentityResult> CreateUserAsync(AppUser user, CancellationToken cancellationToken);
    Task<IdentityResult> RemoveUserAsync(int userId, CancellationToken cancellationToken);
    Task<IdentityResult> UpdateUserAsync(int userId, User user, CancellationToken cancellationToken);
    Task<IdentityResult> CreateRoleAsync(AppRole role, CancellationToken cancellationToken);
    Task<IdentityResult> RemoveRoleAsync(int roleId, CancellationToken cancellationToken);
    Task UserAddToRoleAsync(int userId, int roleRId);
    void RemoveUserFromRole(ManyToManyJoin match);
    Task<IdentityResult> UpdateRoleAsync(int roleId, AppRole role, CancellationToken cancellationToken);
}