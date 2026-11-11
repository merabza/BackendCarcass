using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.ManyToManyJoins;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendCarcass.Application.Tests.Identity;

// UserManager's role membership API over Carcass' store (BackendCarcass#6). AddToRoleAsync threw
// NotImplementedException, or on a case-sensitive database such as SQLite succeeded without adding anything, since
// the store compared the normalized role name UserManager passes with RolKey; RemoveFromRoleAsync threw.
// Membership is a User→Role row of ManyToManyJoins keyed as the rights editor keys it, by the DataTypes' keys:
// PKey = NormalizedUserName (User.Key), CKey = RolKey (Role.Key)
public sealed class MyUserStoreRoleMembershipTests
{
    [Fact]
    public async Task AddToRoleAsync_adds_the_membership_as_the_rights_editor_writes_it()
    {
        using var harness = new IdentityManagersHarness();
        int usersType = harness.DataTypeId(IdentityManagersHarness.UsersTable);
        int rolesType = harness.DataTypeId(IdentityManagersHarness.RolesTable);
        User stored = harness.Db.AddUser("editor", "editor@example.com");
        harness.AddRole("Author");
        Role editorRole = harness.AddRole("Editor");
        AppUser user = await FindAsync(harness, stored);

        IdentityResult result = await harness.UserManager.AddToRoleAsync(user, "Editor");

        Assert.True(result.Succeeded);
        ManyToManyJoin membership = Assert.Single(Memberships(harness));
        Assert.Equal(usersType, membership.PtId);
        Assert.Equal(stored.Key, membership.PKey);
        Assert.Equal(rolesType, membership.CtId);
        Assert.Equal(editorRole.Key, membership.CKey);
        Assert.True(await harness.UserManager.IsInRoleAsync(user, "Editor"));
    }

    [Fact]
    public async Task RemoveFromRoleAsync_removes_the_membership_the_rights_editor_wrote()
    {
        using var harness = new IdentityManagersHarness();
        User stored = harness.Db.AddUser("editor", "editor@example.com");
        User other = harness.Db.AddUser("other", "other@example.com");
        harness.AddRole("Editor");
        harness.AddRole("Admin");
        harness.AddMembership(stored.NormalizedUserName, "Editor");
        harness.AddMembership(stored.NormalizedUserName, "Admin");
        harness.AddMembership(other.NormalizedUserName, "Editor");
        AppUser user = await FindAsync(harness, stored);

        IdentityResult result = await harness.UserManager.RemoveFromRoleAsync(user, "Editor");

        Assert.True(result.Succeeded);
        Assert.False(await harness.UserManager.IsInRoleAsync(user, "Editor"));
        Assert.Equal([(stored.NormalizedUserName, "Admin"), (other.NormalizedUserName, "Editor")],
            Memberships(harness).Select(m => (m.PKey, m.CKey)).Order());
    }

    [Fact]
    public async Task AddToRoleAsync_for_a_member_fails_with_UserAlreadyInRole_and_adds_nothing()
    {
        using var harness = new IdentityManagersHarness();
        AddMembershipDataTypes(harness);
        User stored = harness.Db.AddUser("editor", "editor@example.com");
        harness.AddRole("Editor");
        AppUser user = await FindAsync(harness, stored);
        Assert.True((await harness.UserManager.AddToRoleAsync(user, "Editor")).Succeeded);

        IdentityResult result = await harness.UserManager.AddToRoleAsync(user, "Editor");

        Assert.Equal(nameof(IdentityErrorDescriber.UserAlreadyInRole), Assert.Single(result.Errors).Code);
        Assert.Single(Memberships(harness));
    }

    [Fact]
    public async Task RemoveFromRoleAsync_for_a_non_member_fails_with_UserNotInRole_and_removes_nothing()
    {
        using var harness = new IdentityManagersHarness();
        User stored = harness.Db.AddUser("editor", "editor@example.com");
        User other = harness.Db.AddUser("other", "other@example.com");
        harness.AddRole("Editor");
        harness.AddMembership(other.NormalizedUserName, "Editor");
        AppUser user = await FindAsync(harness, stored);

        IdentityResult result = await harness.UserManager.RemoveFromRoleAsync(user, "Editor");

        Assert.Equal(nameof(IdentityErrorDescriber.UserNotInRole), Assert.Single(result.Errors).Code);
        Assert.Equal(other.NormalizedUserName, Assert.Single(Memberships(harness)).PKey);
    }

    [Fact]
    public async Task A_role_name_in_another_case_finds_the_role()
    {
        using var harness = new IdentityManagersHarness();
        AddMembershipDataTypes(harness);
        User stored = harness.Db.AddUser("editor", "editor@example.com");
        harness.AddRole("Editor");
        AppUser user = await FindAsync(harness, stored);

        IdentityResult added = await harness.UserManager.AddToRoleAsync(user, "eDITOR");
        bool isInRole = await harness.UserManager.IsInRoleAsync(user, "EDITOR");
        string addedRole = Assert.Single(Memberships(harness)).CKey;
        IdentityResult removed = await harness.UserManager.RemoveFromRoleAsync(user, "editor");

        Assert.True(added.Succeeded);
        Assert.True(isInRole);
        Assert.Equal("Editor", addedRole);
        Assert.True(removed.Succeeded);
        Assert.Empty(Memberships(harness));
    }

    [Fact]
    public async Task AddToRoleAsync_for_an_unknown_role_throws_and_adds_nothing()
    {
        using var harness = new IdentityManagersHarness();
        AddMembershipDataTypes(harness);
        User stored = harness.Db.AddUser("editor", "editor@example.com");
        harness.AddRole("Editor");
        AppUser user = await FindAsync(harness, stored);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            harness.UserManager.AddToRoleAsync(user, "Missing"));

        Assert.Empty(Memberships(harness));
    }

    // JwtLoginTokenIssuer puts these roles into the token at login
    [Fact]
    public async Task GetRolesAsync_returns_the_roles_of_the_user_written_by_the_rights_editor_and_the_seed_data()
    {
        using var harness = new IdentityManagersHarness();
        User stored = harness.Db.AddUser("editor", "editor@example.com");
        User other = harness.Db.AddUser("other", "other@example.com");
        harness.AddRole("Editor");
        harness.AddRole("Admin");
        harness.AddRole("Author");
        harness.AddMembership(stored.NormalizedUserName, "Editor");
        harness.AddMembership(stored.UserName, "Admin");
        harness.AddMembership(other.NormalizedUserName, "Author");
        AppUser user = await FindAsync(harness, stored);

        IList<string> roles = await harness.UserManager.GetRolesAsync(user);

        Assert.Equal(["Admin", "Editor"], roles.Order());
    }

    private static async Task<AppUser> FindAsync(IdentityManagersHarness harness, User stored)
    {
        AppUser? user = await harness.UserManager.FindByNameAsync(stored.UserName);
        Assert.NotNull(user);
        return user;
    }

    // The data types of the membership rows exist before anything is added, as in a seeded database
    private static void AddMembershipDataTypes(IdentityManagersHarness harness)
    {
        harness.DataTypeId(IdentityManagersHarness.UsersTable);
        harness.DataTypeId(IdentityManagersHarness.RolesTable);
    }

    private static List<ManyToManyJoin> Memberships(IdentityManagersHarness harness)
    {
        return harness.Db.Context.ManyToManyJoins.AsNoTracking().ToList();
    }
}
