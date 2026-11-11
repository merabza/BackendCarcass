using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.DataTypes;
using BackendCarcass.Domain.ManyToManyJoins;
using BackendCarcass.Domain.Roles;
using BackendCarcass.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendCarcass.Application.Tests.Identity;

// Carcass' Identity store on its own: its reads run on the carcass tables in in-memory SQLite (the repository's
// queryables), its writes go to a mocked IIdentityRepository. The FullName rule of UpdateAsync is covered by
// MyUserStoreUpdateTests
public sealed class MyUserStoreTests : IDisposable
{
    private static readonly DateTimeOffset LockedUntil = new(2026, 12, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly CarcassSqliteDatabase _db = new();
    private readonly CapturingLogger _logger = new();
    private readonly Mock<IIdentityRepository> _repo = new();
    private readonly MyUserStore _sut;

    public MyUserStoreTests()
    {
        _repo.Setup(r => r.Users).Returns(_db.Context.Users);
        _repo.Setup(r => r.Roles).Returns(_db.Context.Roles);
        _repo.Setup(r => r.RolesByUsers).Returns(_db.Context.ManyToManyJoins);
        _sut = new MyUserStore(_repo.Object, _logger);
    }

    private IRoleStore<AppRole> RoleStore => _sut;

    public void Dispose()
    {
        _sut.Dispose();
        _db.Dispose();
    }

    [Fact]
    public async Task CreateAsync_Role_ReturnsTheRepositoryResult()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50);
        IdentityResult expected = Failure();
        _repo.Setup(r => r.CreateRoleAsync(role, CancellationToken.None)).ReturnsAsync(expected);

        IdentityResult result = await _sut.CreateAsync(role, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task DeleteAsync_Role_RemovesTheRoleByIdAndReturnsTheRepositoryResult()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50) { Id = 7 };
        IdentityResult expected = Failure();
        _repo.Setup(r => r.RemoveRoleAsync(7, CancellationToken.None)).ReturnsAsync(expected);

        IdentityResult result = await _sut.DeleteAsync(role, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task UpdateAsync_Role_UpdatesTheRoleByIdAndReturnsTheRepositoryResult()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50) { Id = 7 };
        IdentityResult expected = Failure();
        _repo.Setup(r => r.UpdateRoleAsync(7, role, CancellationToken.None)).ReturnsAsync(expected);

        IdentityResult result = await _sut.UpdateAsync(role, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetRoleIdAsync_ReturnsTheIdAsText()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50) { Id = 7 };

        string id = await _sut.GetRoleIdAsync(role, CancellationToken.None);

        Assert.Equal("7", id);
    }

    [Fact]
    public async Task GetRoleNameAsync_ReturnsTheName()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50);

        string? name = await _sut.GetRoleNameAsync(role, CancellationToken.None);

        Assert.Equal("Editor", name);
    }

    [Fact]
    public async Task SetRoleNameAsync_SetsTheName()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50);

        await _sut.SetRoleNameAsync(role, "Author", CancellationToken.None);

        Assert.Equal("Author", role.Name);
    }

    [Fact]
    public async Task GetNormalizedRoleNameAsync_ReturnsTheNormalizedName()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50) { NormalizedName = "EDITOR" };

        string? name = await _sut.GetNormalizedRoleNameAsync(role, CancellationToken.None);

        Assert.Equal("EDITOR", name);
    }

    [Fact]
    public async Task SetNormalizedRoleNameAsync_SetsTheNormalizedName()
    {
        var role = new AppRole("Editor", "რედაქტორი", 50);

        await _sut.SetNormalizedRoleNameAsync(role, "AUTHOR", CancellationToken.None);

        Assert.Equal("AUTHOR", role.NormalizedName);
    }

    [Fact]
    public void Roles_ProjectsEveryStoredRole()
    {
        Role editor = AddRole("Editor", 50);
        AddRole("Author", 40);

        List<AppRole> roles = _sut.Roles.ToList();

        Assert.Equal(2, roles.Count);
        AssertRole(editor, Assert.Single(roles, r => r.Name == "Editor"));
    }

    [Fact]
    public async Task FindByIdAsync_ExistingRoleId_ReturnsTheRole()
    {
        Role editor = AddRole("Editor", 50);
        AddRole("Author", 40);

        AppRole? role = await RoleStore.FindByIdAsync(Text(editor.RolId), CancellationToken.None);

        AssertRole(editor, role);
    }

    [Fact]
    public async Task FindByIdAsync_RoleIdIsNotANumber_ReturnsNull()
    {
        AddRole("Editor", 50);

        AppRole? role = await RoleStore.FindByIdAsync("Editor", CancellationToken.None);

        Assert.Null(role);
    }

    [Fact]
    public async Task FindByIdAsync_UnknownRoleId_ReturnsNull()
    {
        Role editor = AddRole("Editor", 50);

        AppRole? role = await RoleStore.FindByIdAsync(Text(editor.RolId + 1), CancellationToken.None);

        Assert.Null(role);
    }

    [Fact]
    public async Task FindByNameAsync_ExistingNormalizedRoleName_ReturnsTheRole()
    {
        Role editor = AddRole("Editor", 50);
        AddRole("Author", 40);

        AppRole? role = await RoleStore.FindByNameAsync("EDITOR", CancellationToken.None);

        AssertRole(editor, role);
    }

    [Fact]
    public async Task FindByNameAsync_UnknownRoleName_ReturnsNull()
    {
        AddRole("Editor", 50);

        AppRole? role = await RoleStore.FindByNameAsync("AUTHOR", CancellationToken.None);

        Assert.Null(role);
    }

    [Fact]
    public void Users_ProjectsEveryStoredUser()
    {
        User tester = AddUser("tester");
        AddUser("other");

        List<AppUser> users = _sut.Users.ToList();

        Assert.Equal(2, users.Count);
        AssertUser(tester, Assert.Single(users, u => u.UserName == "tester"));
    }

    [Fact]
    public void Users_InformationLoggingEnabled_LogsTheUserCount()
    {
        AddUser("tester");
        AddUser("other");
        _logger.InformationEnabled = true;

        _ = _sut.Users;

        Assert.Equal((LogLevel.Information, "User count: 2"), Assert.Single(_logger.Entries));
    }

    [Fact]
    public void Users_InformationLoggingDisabled_LogsNothing()
    {
        AddUser("tester");

        _ = _sut.Users;

        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task SetEmailAsync_SetsTheEmail()
    {
        AppUser user = NewAppUser();

        await _sut.SetEmailAsync(user, "new@example.com", CancellationToken.None);

        Assert.Equal("new@example.com", user.Email);
    }

    [Fact]
    public async Task GetEmailAsync_ReturnsTheEmail()
    {
        AppUser user = NewAppUser();

        string? email = await _sut.GetEmailAsync(user, CancellationToken.None);

        Assert.Equal("tester@example.com", email);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetEmailConfirmedAsync_ReturnsTheFlag(bool confirmed)
    {
        AppUser user = NewAppUser();
        user.EmailConfirmed = confirmed;

        bool result = await _sut.GetEmailConfirmedAsync(user, CancellationToken.None);

        Assert.Equal(confirmed, result);
    }

    [Fact]
    public async Task SetEmailConfirmedAsync_SetsTheFlag()
    {
        AppUser user = NewAppUser();

        await _sut.SetEmailConfirmedAsync(user, true, CancellationToken.None);

        Assert.True(user.EmailConfirmed);
    }

    [Fact]
    public async Task FindByEmailAsync_ExistingNormalizedEmail_ReturnsTheUser()
    {
        User tester = AddUser("tester");
        AddUser("other");

        AppUser? user = await _sut.FindByEmailAsync("TESTER@EXAMPLE.COM", CancellationToken.None);

        AssertUser(tester, user);
    }

    [Fact]
    public async Task FindByEmailAsync_UnknownEmail_ReturnsNull()
    {
        AddUser("tester");

        AppUser? user = await _sut.FindByEmailAsync("NOBODY@EXAMPLE.COM", CancellationToken.None);

        Assert.Null(user);
    }

    [Fact]
    public async Task GetNormalizedEmailAsync_ReturnsTheNormalizedEmail()
    {
        AppUser user = NewAppUser();

        string? email = await _sut.GetNormalizedEmailAsync(user, CancellationToken.None);

        Assert.Equal("TESTER@EXAMPLE.COM", email);
    }

    [Fact]
    public async Task SetNormalizedEmailAsync_SetsTheNormalizedEmail()
    {
        AppUser user = NewAppUser();

        await _sut.SetNormalizedEmailAsync(user, "NEW@EXAMPLE.COM", CancellationToken.None);

        Assert.Equal("NEW@EXAMPLE.COM", user.NormalizedEmail);
    }

    [Fact]
    public async Task CreateAsync_User_ReturnsTheRepositoryResult()
    {
        AppUser user = NewAppUser();
        IdentityResult expected = Failure();
        _repo.Setup(r => r.CreateUserAsync(user, CancellationToken.None)).ReturnsAsync(expected);

        IdentityResult result = await _sut.CreateAsync(user, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task DeleteAsync_User_RemovesTheUserByIdAndReturnsTheRepositoryResult()
    {
        AppUser user = NewAppUser();
        IdentityResult expected = Failure();
        _repo.Setup(r => r.RemoveUserAsync(7, CancellationToken.None)).ReturnsAsync(expected);

        IdentityResult result = await _sut.DeleteAsync(user, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task FindByIdAsync_ExistingUserId_ReturnsTheUser()
    {
        User tester = AddUser("tester");
        AddUser("other");

        AppUser? user = await _sut.FindByIdAsync(Text(tester.UsrId), CancellationToken.None);

        AssertUser(tester, user);
    }

    [Fact]
    public async Task FindByIdAsync_UserIdIsNotANumber_ReturnsNull()
    {
        AddUser("tester");

        AppUser? user = await _sut.FindByIdAsync("tester", CancellationToken.None);

        Assert.Null(user);
    }

    [Fact]
    public async Task FindByIdAsync_UnknownUserId_ReturnsNull()
    {
        User tester = AddUser("tester");

        AppUser? user = await _sut.FindByIdAsync(Text(tester.UsrId + 1), CancellationToken.None);

        Assert.Null(user);
    }

    [Fact]
    public async Task FindByNameAsync_ExistingNormalizedUserName_ReturnsTheUser()
    {
        User tester = AddUser("tester");
        AddUser("other");

        AppUser? user = await _sut.FindByNameAsync("TESTER", CancellationToken.None);

        AssertUser(tester, user);
    }

    [Fact]
    public async Task FindByNameAsync_UnknownUserName_ReturnsNull()
    {
        AddUser("tester");

        AppUser? user = await _sut.FindByNameAsync("NOBODY", CancellationToken.None);

        Assert.Null(user);
    }

    [Fact]
    public async Task FindByNameAsync_CancelledToken_ThrowsBeforeQuerying()
    {
        AddUser("tester");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.FindByNameAsync("TESTER", cancellation.Token));

        _repo.VerifyGet(r => r.Users, Times.Never);
    }

    [Fact]
    public async Task GetNormalizedUserNameAsync_ReturnsTheNormalizedUserName()
    {
        AppUser user = NewAppUser();

        string? userName = await _sut.GetNormalizedUserNameAsync(user, CancellationToken.None);

        Assert.Equal("TESTER", userName);
    }

    [Fact]
    public async Task GetUserIdAsync_ReturnsTheIdAsText()
    {
        AppUser user = NewAppUser();

        string id = await _sut.GetUserIdAsync(user, CancellationToken.None);

        Assert.Equal("7", id);
    }

    [Fact]
    public async Task GetUserNameAsync_ReturnsTheUserName()
    {
        AppUser user = NewAppUser();

        string? userName = await _sut.GetUserNameAsync(user, CancellationToken.None);

        Assert.Equal("tester", userName);
    }

    [Fact]
    public async Task SetNormalizedUserNameAsync_SetsTheNormalizedUserName()
    {
        AppUser user = NewAppUser();

        await _sut.SetNormalizedUserNameAsync(user, "RENAMED", CancellationToken.None);

        Assert.Equal("RENAMED", user.NormalizedUserName);
    }

    [Fact]
    public async Task SetUserNameAsync_SetsTheUserName()
    {
        AppUser user = NewAppUser();

        await _sut.SetUserNameAsync(user, "renamed", CancellationToken.None);

        Assert.Equal("renamed", user.UserName);
    }

    [Fact]
    public async Task UpdateAsync_UnknownUser_FailsWithoutWriting()
    {
        User tester = AddUser("tester");
        AppUser user = AppUserFor(tester);
        user.Id = tester.UsrId + 1;

        IdentityResult result = await _sut.UpdateAsync(user, CancellationToken.None);

        Assert.False(result.Succeeded);
        VerifyNoUserUpdate();
    }

    [Theory]
    [InlineData(nameof(AppUser.UserName))]
    [InlineData(nameof(AppUser.NormalizedUserName))]
    [InlineData(nameof(AppUser.Email))]
    [InlineData(nameof(AppUser.NormalizedEmail))]
    [InlineData(nameof(AppUser.PasswordHash))]
    public async Task UpdateAsync_RequiredFieldIsBlank_FailsWithoutWriting(string field)
    {
        User tester = AddUser("tester");
        AppUser user = AppUserFor(tester);
        typeof(AppUser).GetProperty(field)!.SetValue(user, " ");

        IdentityResult result = await _sut.UpdateAsync(user, CancellationToken.None);

        Assert.False(result.Succeeded);
        VerifyNoUserUpdate();
    }

    [Fact]
    public async Task UpdateAsync_ValidUser_WritesEveryFieldToTheStoredUser()
    {
        User tester = AddUser("tester");
        var user = new AppUser("renamed", "ნინო", "ბერიძე")
        {
            Id = tester.UsrId,
            NormalizedUserName = "RENAMED",
            Email = "nino@example.com",
            NormalizedEmail = "NINO@EXAMPLE.COM",
            PasswordHash = "new hash",
            EmailConfirmed = false,
            AccessFailedCount = 4,
            LockoutEnd = null
        };
        User? written = null;
        _repo.Setup(r => r.UpdateUserAsync(tester.UsrId, It.IsAny<User>(), CancellationToken.None))
            .Callback((int _, User stored, CancellationToken _) => written = stored)
            .ReturnsAsync(IdentityResult.Success);

        await _sut.UpdateAsync(user, CancellationToken.None);

        Assert.NotNull(written);
        Assert.Equal(tester.UsrId, written.UsrId);
        Assert.Equal("renamed", written.UserName);
        Assert.Equal("RENAMED", written.NormalizedUserName);
        Assert.Equal("nino@example.com", written.Email);
        Assert.Equal("NINO@EXAMPLE.COM", written.NormalizedEmail);
        Assert.Equal("new hash", written.PasswordHash);
        Assert.Equal("ნინო", written.FirstName);
        Assert.Equal("ბერიძე", written.LastName);
        Assert.False(written.EmailConfirmed);
        Assert.Equal(4, written.AccessFailedCount);
        Assert.Null(written.LockoutEnd);
    }

    [Fact]
    public async Task UpdateAsync_ValidUser_ReturnsTheRepositoryResult()
    {
        User tester = AddUser("tester");
        IdentityResult expected = Failure();
        _repo.Setup(r => r.UpdateUserAsync(tester.UsrId, It.IsAny<User>(), CancellationToken.None))
            .ReturnsAsync(expected);

        IdentityResult result = await _sut.UpdateAsync(AppUserFor(tester), CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task SetPasswordHashAsync_SetsTheHash()
    {
        AppUser user = NewAppUser();

        await _sut.SetPasswordHashAsync(user, "new hash", CancellationToken.None);

        Assert.Equal("new hash", user.PasswordHash);
    }

    [Fact]
    public async Task GetPasswordHashAsync_KnownUser_ReturnsTheStoredHash()
    {
        User tester = AddUser("tester");
        AddUser("other");

        string? hash = await _sut.GetPasswordHashAsync(AppUserFor(tester), CancellationToken.None);

        Assert.Equal("tester hash", hash);
    }

    [Fact]
    public async Task GetPasswordHashAsync_UnknownUser_ReturnsNull()
    {
        AddUser("tester");

        string? hash = await _sut.GetPasswordHashAsync(NewAppUser("nobody"), CancellationToken.None);

        Assert.Null(hash);
    }

    [Fact]
    public async Task HasPasswordAsync_StoredHashIsSet_ReturnsTrue()
    {
        User tester = AddUser("tester");
        SetStoredHash(AddUser("other"), " ");

        bool hasPassword = await _sut.HasPasswordAsync(AppUserFor(tester), CancellationToken.None);

        Assert.True(hasPassword);
    }

    [Fact]
    public async Task HasPasswordAsync_StoredHashIsBlank_ReturnsFalse()
    {
        User tester = AddUser("tester");
        SetStoredHash(tester, " ");
        AddUser("other");

        bool hasPassword = await _sut.HasPasswordAsync(AppUserFor(tester), CancellationToken.None);

        Assert.False(hasPassword);
    }

    [Fact]
    public async Task HasPasswordAsync_UnknownUser_ReturnsFalse()
    {
        AddUser("tester");

        bool hasPassword = await _sut.HasPasswordAsync(NewAppUser("nobody"), CancellationToken.None);

        Assert.False(hasPassword);
    }

    [Fact]
    public async Task GetLockoutEndDateAsync_ReturnsTheLockoutEnd()
    {
        AppUser user = NewAppUser();
        user.LockoutEnd = LockedUntil;

        DateTimeOffset? lockoutEnd = await _sut.GetLockoutEndDateAsync(user, CancellationToken.None);

        Assert.Equal(LockedUntil, lockoutEnd);
    }

    [Fact]
    public async Task SetLockoutEndDateAsync_SetsTheLockoutEnd()
    {
        AppUser user = NewAppUser();

        await _sut.SetLockoutEndDateAsync(user, LockedUntil, CancellationToken.None);

        Assert.Equal(LockedUntil, user.LockoutEnd);
    }

    [Fact]
    public async Task IncrementAccessFailedCountAsync_AddsOneAndReturnsTheNewCount()
    {
        AppUser user = NewAppUser();
        user.AccessFailedCount = 2;

        int count = await _sut.IncrementAccessFailedCountAsync(user, CancellationToken.None);

        Assert.Equal(3, count);
        Assert.Equal(3, user.AccessFailedCount);
    }

    [Fact]
    public async Task ResetAccessFailedCountAsync_SetsTheCountToZero()
    {
        AppUser user = NewAppUser();
        user.AccessFailedCount = 2;

        await _sut.ResetAccessFailedCountAsync(user, CancellationToken.None);

        Assert.Equal(0, user.AccessFailedCount);
    }

    [Fact]
    public async Task GetAccessFailedCountAsync_ReturnsTheCount()
    {
        AppUser user = NewAppUser();
        user.AccessFailedCount = 2;

        int count = await _sut.GetAccessFailedCountAsync(user, CancellationToken.None);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetLockoutEnabledAsync_ReturnsTrueForEveryUser()
    {
        bool enabled = await _sut.GetLockoutEnabledAsync(NewAppUser(), CancellationToken.None);

        Assert.True(enabled);
    }

    [Fact]
    public async Task SetLockoutEnabledAsync_Disabling_KeepsLockoutEnabled()
    {
        AppUser user = NewAppUser();

        await _sut.SetLockoutEnabledAsync(user, false, CancellationToken.None);

        Assert.True(await _sut.GetLockoutEnabledAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task AddToRoleAsync_ExistingRole_AddsTheUserToIt()
    {
        AddRole("Author", 40);
        Role editor = AddRole("Editor", 50);

        await _sut.AddToRoleAsync(NewAppUser(), "Editor", CancellationToken.None);

        _repo.Verify(r => r.UserAddToRoleAsync(7, editor.RolId), Times.Once);
        _repo.Verify(r => r.UserAddToRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task AddToRoleAsync_UnknownRole_AddsNothing()
    {
        AddRole("Editor", 50);

        await _sut.AddToRoleAsync(NewAppUser(), "Author", CancellationToken.None);

        _repo.Verify(r => r.UserAddToRoleAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFromRoleAsync_UserIsInTheRole_RemovesThatMembership()
    {
        AddRole("Editor", 50);
        AddRole("Author", 40);
        AddUserToRole("other", "Editor");
        AddUserToRole("tester", "Author");
        AddUserToRole("tester", "Editor");
        ManyToManyJoin? removed = null;
        _repo.Setup(r => r.RemoveUserFromRole(It.IsAny<ManyToManyJoin>()))
            .Callback((ManyToManyJoin membership) => removed = membership);

        await _sut.RemoveFromRoleAsync(NewAppUser(), "Editor", CancellationToken.None);

        Assert.NotNull(removed);
        Assert.Equal("tester", removed.PKey);
        Assert.Equal("Editor", removed.CKey);
    }

    [Fact]
    public async Task RemoveFromRoleAsync_UnknownRole_RemovesNothing()
    {
        AddUserToRole("tester", "Editor");

        await _sut.RemoveFromRoleAsync(NewAppUser(), "Editor", CancellationToken.None);

        VerifyNoRoleRemoval();
    }

    [Fact]
    public async Task RemoveFromRoleAsync_UserIsNotInTheRole_RemovesNothing()
    {
        AddRole("Editor", 50);
        AddUserToRole("other", "Editor");

        await _sut.RemoveFromRoleAsync(NewAppUser(), "Editor", CancellationToken.None);

        VerifyNoRoleRemoval();
    }

    [Fact]
    public async Task GetRolesAsync_ReturnsTheRolesOfThisUserOnly()
    {
        AddUserToRole("tester", "Editor");
        AddUserToRole("tester", "Author");
        AddUserToRole("other", "Admin");

        IList<string> roles = await _sut.GetRolesAsync(NewAppUser(), CancellationToken.None);

        Assert.Equal(2, roles.Count);
        Assert.Contains("Editor", roles);
        Assert.Contains("Author", roles);
    }

    [Fact]
    public async Task IsInRoleAsync_UserIsInTheRole_ReturnsTrue()
    {
        AddRole("Editor", 50);
        AddRole("Author", 40);
        AddUserToRole("tester", "Editor");

        bool isInRole = await _sut.IsInRoleAsync(NewAppUser(), "Editor", CancellationToken.None);

        Assert.True(isInRole);
    }

    [Fact]
    public async Task IsInRoleAsync_UserIsNotInTheRole_ReturnsFalse()
    {
        AddRole("Editor", 50);
        AddRole("Author", 40);
        AddUserToRole("tester", "Author");
        AddUserToRole("other", "Editor");

        bool isInRole = await _sut.IsInRoleAsync(NewAppUser(), "Editor", CancellationToken.None);

        Assert.False(isInRole);
    }

    [Fact]
    public async Task IsInRoleAsync_UnknownRole_ReturnsFalse()
    {
        AddUserToRole("tester", "Editor");

        bool isInRole = await _sut.IsInRoleAsync(NewAppUser(), "Editor", CancellationToken.None);

        Assert.False(isInRole);
    }

    // Documents the current behavior: the store ignores the role name and returns every user
    [Fact]
    public async Task GetUsersInRoleAsync_IgnoresTheRoleAndReturnsEveryUser()
    {
        AddUser("tester");
        User other = AddUser("other");
        AddRole("Editor", 50);
        AddUserToRole("tester", "Editor");

        IList<AppUser> users =
            await ((IUserRoleStore<AppUser>)_sut).GetUsersInRoleAsync("EDITOR", CancellationToken.None);

        Assert.Equal(2, users.Count);
        Assert.Equal("OTHER", Assert.Single(users, u => u.Id == other.UsrId).NormalizedUserName);
    }

    private static IdentityResult Failure()
    {
        return IdentityResult.Failed(new IdentityError { Code = "RepositoryError", Description = "repository" });
    }

    private static string Text(int id)
    {
        return id.ToString(CultureInfo.InvariantCulture);
    }

    private static AppUser NewAppUser(string userName = "tester")
    {
        string email = userName + "@example.com";
        return new AppUser(userName, "Test", "User")
        {
            Id = 7,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = userName + " hash"
        };
    }

    private static AppUser AppUserFor(User user)
    {
        return new AppUser(user.UserName, user.FirstName, user.LastName)
        {
            Id = user.UsrId,
            NormalizedUserName = user.NormalizedUserName,
            Email = user.Email,
            NormalizedEmail = user.NormalizedEmail,
            PasswordHash = user.PasswordHash
        };
    }

    private static void AssertUser(User expected, AppUser? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.UsrId, actual.Id);
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
    }

    private static void AssertRole(Role expected, AppRole? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.RolId, actual.Id);
        Assert.Equal(expected.RolKey, actual.Name);
        Assert.Equal(expected.RolName, actual.RoleName);
        Assert.Equal(expected.RolLevel, actual.Level);
        Assert.Equal(expected.RolNormalizedKey, actual.NormalizedName);
    }

    // A stored user whose mapped fields all differ from their defaults
    private User AddUser(string userName)
    {
        User user = _db.AddUser(userName, userName + "@example.com");
        user.PasswordHash = userName + " hash";
        user.EmailConfirmed = true;
        user.AccessFailedCount = 2;
        user.LockoutEnd = LockedUntil;
        _db.Context.SaveChanges();
        return user;
    }

    private void SetStoredHash(User user, string passwordHash)
    {
        user.PasswordHash = passwordHash;
        _db.Context.SaveChanges();
    }

    private Role AddRole(string key, int level)
    {
        var role = new Role
        {
            RolKey = key, RolName = key + " როლი", RolLevel = level, RolNormalizedKey = key.ToUpperInvariant()
        };
        _db.Context.Roles.Add(role);
        _db.Context.SaveChanges();
        return role;
    }

    // A User→Role membership row of ManyToManyJoins, as the carcass keeps it (by user name and role key)
    private void AddUserToRole(string userName, string roleKey)
    {
        int usersType = DataTypeId("Users");
        int rolesType = DataTypeId("Roles");
        _db.Context.ManyToManyJoins.Add(new ManyToManyJoin
        {
            PtId = usersType, PKey = userName, CtId = rolesType, CKey = roleKey
        });
        _db.Context.SaveChanges();
    }

    private int DataTypeId(string table)
    {
        DataType? dataType = _db.Context.DataTypes.SingleOrDefault(d => d.DtTable == table);
        if (dataType is null)
        {
            dataType = new DataType { DtName = table, DtNameNominative = table, DtNameGenitive = table, DtTable = table };
            _db.Context.DataTypes.Add(dataType);
            _db.Context.SaveChanges();
        }

        return dataType.DtId;
    }

    private void VerifyNoUserUpdate()
    {
        _repo.Verify(r => r.UpdateUserAsync(It.IsAny<int>(), It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void VerifyNoRoleRemoval()
    {
        _repo.Verify(r => r.RemoveUserFromRole(It.IsAny<ManyToManyJoin>()), Times.Never);
    }

    // Records what the store logs; logging is off unless a test turns it on
    private sealed class CapturingLogger : ILogger<MyUserStore>
    {
        public bool InformationEnabled { get; set; }

        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return InformationEnabled;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
