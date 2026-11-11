using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.Users;
using Xunit;

namespace BackendCarcass.Application.Tests.Identity;

// UserManager.GetUsersInRoleAsync over Carcass' store returns the members of that role only (the store ignored the
// role and returned every user, half-filled, BackendCarcass#5). Membership is a User→Role row of ManyToManyJoins, as
// the rights editor writes it: PKey = NormalizedUserName, CKey = RolKey
public sealed class MyUserStoreUsersInRoleTests
{
    [Fact]
    public async Task Returns_the_members_of_the_role_fully_filled()
    {
        using var harness = new IdentityManagersHarness();
        User editor = AddUser(harness, "editor");
        User secondEditor = AddUser(harness, "second");
        AddUser(harness, "outsider");
        harness.AddRole("Editor");
        harness.AddMembership(editor.NormalizedUserName, "Editor");
        harness.AddMembership(secondEditor.NormalizedUserName, "Editor");

        IList<AppUser> users = await harness.UserManager.GetUsersInRoleAsync("Editor");

        Assert.Equal([editor.UsrId, secondEditor.UsrId], users.Select(u => u.Id).Order());
        foreach (AppUser user in users)
        {
            AppUser? found = await harness.UserManager.FindByIdAsync(user.Id.ToString(CultureInfo.InvariantCulture));
            AssertSameStoredFields(found, user);
        }
    }

    // ConcurrencyStamp and SecurityStamp are not stored, every AppUser gets new ones
    private static void AssertSameStoredFields(AppUser? expected, AppUser actual)
    {
        Assert.NotNull(expected);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.UserName, actual.UserName);
        Assert.Equal(expected.NormalizedUserName, actual.NormalizedUserName);
        Assert.Equal(expected.Email, actual.Email);
        Assert.Equal(expected.NormalizedEmail, actual.NormalizedEmail);
        Assert.Equal(expected.PasswordHash, actual.PasswordHash);
        Assert.Equal(expected.FirstName, actual.FirstName);
        Assert.Equal(expected.LastName, actual.LastName);
        Assert.Equal(expected.EmailConfirmed, actual.EmailConfirmed);
        Assert.Equal(expected.AccessFailedCount, actual.AccessFailedCount);
        Assert.Equal(expected.LockoutEnd, actual.LockoutEnd);
        Assert.NotNull(actual.Email);
    }

    [Fact]
    public async Task Leaves_out_the_members_of_other_roles()
    {
        using var harness = new IdentityManagersHarness();
        User editor = AddUser(harness, "editor");
        User admin = AddUser(harness, "admin");
        harness.AddRole("Editor");
        harness.AddRole("Admin");
        harness.AddMembership(editor.NormalizedUserName, "Editor");
        harness.AddMembership(admin.NormalizedUserName, "Admin");

        IList<AppUser> users = await harness.UserManager.GetUsersInRoleAsync("Editor");

        Assert.Equal(editor.UsrId, Assert.Single(users).Id);
    }

    [Fact]
    public async Task A_role_without_members_returns_an_empty_list()
    {
        using var harness = new IdentityManagersHarness();
        AddUser(harness, "editor");
        AddUser(harness, "other");
        harness.AddRole("Editor");

        IList<AppUser> users = await harness.UserManager.GetUsersInRoleAsync("Editor");

        Assert.Empty(users);
    }

    [Fact]
    public async Task An_unknown_role_returns_an_empty_list()
    {
        using var harness = new IdentityManagersHarness();
        User editor = AddUser(harness, "editor");
        harness.AddRole("Editor");
        harness.AddMembership(editor.NormalizedUserName, "Editor");

        IList<AppUser> users = await harness.UserManager.GetUsersInRoleAsync("Missing");

        Assert.Empty(users);
    }

    private static User AddUser(IdentityManagersHarness harness, string userName)
    {
        return harness.Db.AddUser(userName, userName + "@example.com");
    }
}
