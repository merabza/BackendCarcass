using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.MasterData.Models;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.Users;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using Moq;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.Repositories;

// UsersMdRepo answers what Identity answered: success after a successful operation (the delete of the current user
// answered 400 after deleting it, BackendCarcass#3) and Identity's errors after a failed one
public sealed class UsersMdRepoTests
{
    [Fact]
    public async Task Delete_of_an_existing_user_succeeds_and_removes_the_row()
    {
        using var harness = new IdentityManagersHarness();
        User user = harness.Db.AddUser();
        var repo = new UsersMdRepo(harness.UserManager);

        Result result = await repo.Delete(user.UsrId);

        Assert.True(result.IsSuccess);
        Assert.Empty(harness.Db.Context.Users);
    }

    [Fact]
    public async Task Delete_fails_when_the_store_cannot_delete_the_user()
    {
        using var harness = new IdentityManagersHarness();
        User user = harness.Db.AddUser();
        harness.ForbidUserDeletes();
        var repo = new UsersMdRepo(harness.UserManager);

        Result result = await repo.Delete(user.UsrId);

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
    }

    [Fact]
    public async Task Delete_of_a_missing_user_answers_that_the_user_cannot_be_found()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new UsersMdRepo(harness.UserManager);

        Result result = await repo.Delete(12345);

        Assert.Equal(MasterDataApiErrors.CannotFindUser, result.Error);
    }

    [Fact]
    public async Task Update_succeeds_and_stores_the_new_fields()
    {
        using var harness = new IdentityManagersHarness();
        User user = harness.Db.AddUser();
        var repo = new UsersMdRepo(harness.UserManager);

        Result result = await repo.Update(user.UsrId,
            NewUser("renamed", "renamed@example.com", "ახალი", "გვარი"));

        Assert.True(result.IsSuccess);
        User stored = harness.Db.Context.Users.Single();
        Assert.Equal("renamed", stored.UserName);
        Assert.Equal("RENAMED", stored.NormalizedUserName);
        Assert.Equal("renamed@example.com", stored.Email);
        Assert.Equal("ახალი", stored.FirstName);
        Assert.Equal("გვარი", stored.LastName);
    }

    [Fact]
    public async Task Update_to_a_taken_user_name_fails_with_identitys_error()
    {
        using var harness = new IdentityManagersHarness();
        harness.Db.AddUser("taken", "taken@example.com");
        User user = harness.Db.AddUser();
        var repo = new UsersMdRepo(harness.UserManager);

        Result result = await repo.Update(user.UsrId, NewUser("taken", "tester@example.com", "Test", "User"));

        Assert.True(result.IsFailure);
        Assert.Contains(Errors(result), e => e.Code == "DuplicateUserName");
    }

    [Fact]
    public async Task Create_succeeds_when_identity_creates_the_user()
    {
        Mock<UserManager<AppUser>> userManager = MockUserManager();
        userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync((AppUser appUser) =>
            {
                appUser.Id = 7;
                return IdentityResult.Success;
            });
        var repo = new UsersMdRepo(userManager.Object);
        User user = NewUser("new", "new@example.com", "Test", "User");

        Result result = await repo.Create(user);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, user.UsrId);
    }

    [Fact]
    public async Task Create_fails_with_identitys_errors()
    {
        Mock<UserManager<AppUser>> userManager = MockUserManager();
        userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidUserName", Description = "ცუდი" }));
        var repo = new UsersMdRepo(userManager.Object);

        Result result = await repo.Create(NewUser("bad name", "new@example.com", "Test", "User"));

        Assert.True(result.IsFailure);
        Error error = Assert.Single(Errors(result));
        Assert.Equal("InvalidUserName", error.Code);
        Assert.Equal("ცუდი", error.Description);
    }

    private static Error[] Errors(Result result)
    {
        return Assert.IsType<ValidationError>(result.Error).Errors;
    }

    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        return new Mock<UserManager<AppUser>>(Mock.Of<IUserStore<AppUser>>(), null!, null!, null!, null!, null!,
            null!, null!, null!);
    }

    private static User NewUser(string userName, string email, string firstName, string lastName)
    {
        return new User
        {
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = string.Empty,
            FullName = $"{firstName} {lastName}",
            FirstName = firstName,
            LastName = lastName
        };
    }
}
