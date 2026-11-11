using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendCarcass.Application.Tests.Identity;

// UserManager.UpdateAsync over Carcass' store writes FullName = FirstName + " " + LastName when a name changes (the
// store wrote the new names but kept the old FullName, BackendCarcass#4). An update of other fields keeps the stored
// FullName: the old-data convertors import it apart from the names, which may be empty
public sealed class MyUserStoreUpdateTests
{
    private const string ImportedFullName = "გიორგი მაისურაძე";

    [Theory]
    [InlineData("ნინო", "User", "ნინო User")]
    [InlineData("Test", "ბერიძე", "Test ბერიძე")]
    [InlineData("ნინო", "ბერიძე", "ნინო ბერიძე")]
    public async Task Changing_a_name_rewrites_the_full_name(string firstName, string lastName,
        string expectedFullName)
    {
        using var harness = new IdentityManagersHarness();
        harness.Db.AddUser();
        AppUser user = await FindTester(harness);
        user.FirstName = firstName;
        user.LastName = lastName;

        IdentityResult result = await harness.UserManager.UpdateAsync(user);

        Assert.True(result.Succeeded);
        User stored = await StoredUser(harness);
        Assert.Equal(firstName, stored.FirstName);
        Assert.Equal(lastName, stored.LastName);
        Assert.Equal(expectedFullName, stored.FullName);
    }

    [Fact]
    public async Task Updating_other_fields_keeps_the_full_name()
    {
        using var harness = new IdentityManagersHarness();
        AddImportedUser(harness);
        AppUser user = await FindTester(harness);
        user.Email = "giorgi@example.com";
        user.EmailConfirmed = true;

        IdentityResult result = await harness.UserManager.UpdateAsync(user);

        Assert.True(result.Succeeded);
        User stored = await StoredUser(harness);
        Assert.Equal("giorgi@example.com", stored.Email);
        Assert.True(stored.EmailConfirmed);
        Assert.Equal(ImportedFullName, stored.FullName);
    }

    [Fact]
    public async Task A_failed_login_keeps_the_full_name()
    {
        using var harness = new IdentityManagersHarness();
        AddImportedUser(harness);
        AppUser user = await FindTester(harness);

        IdentityResult result = await harness.UserManager.AccessFailedAsync(user);

        Assert.True(result.Succeeded);
        User stored = await StoredUser(harness);
        Assert.Equal(1, stored.AccessFailedCount);
        Assert.Equal(ImportedFullName, stored.FullName);
    }

    // A user as GrammarGe's OldDataConvertor imports one: the full name from the old database, the names empty
    private static void AddImportedUser(IdentityManagersHarness harness)
    {
        User user = harness.Db.AddUser();
        user.FirstName = string.Empty;
        user.LastName = string.Empty;
        user.FullName = ImportedFullName;
        harness.Db.Context.SaveChanges();
    }

    private static async Task<AppUser> FindTester(IdentityManagersHarness harness)
    {
        AppUser? user = await harness.UserManager.FindByNameAsync("tester");
        Assert.NotNull(user);
        return user;
    }

    // The row as SaveChanges wrote it, not the instance the context tracks
    private static Task<User> StoredUser(IdentityManagersHarness harness)
    {
        return harness.Db.Context.Users.AsNoTracking().SingleAsync();
    }
}
