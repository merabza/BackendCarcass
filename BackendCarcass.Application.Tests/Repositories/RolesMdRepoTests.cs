using System.Linq;
using System.Threading.Tasks;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Domain.Roles;
using BackendCarcassShared.Contracts.Errors;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.Repositories;

// RolesMdRepo answers what Identity answered: success after a successful operation and Identity's errors after a
// failed one (BackendCarcass#3)
public sealed class RolesMdRepoTests
{
    [Fact]
    public async Task Create_succeeds_and_stores_the_role()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new RolesMdRepo(harness.RoleManager);
        Role role = NewRole("Editor", "რედაქტორი", 50);

        Result result = await repo.Create(role);

        Assert.True(result.IsSuccess);
        Role stored = harness.Db.Context.Roles.Single();
        Assert.Equal(stored.RolId, role.RolId);
        Assert.Equal("Editor", stored.RolKey);
        Assert.Equal("EDITOR", stored.RolNormalizedKey);
        Assert.Equal("რედაქტორი", stored.RolName);
        Assert.Equal(50, stored.RolLevel);
    }

    [Fact]
    public async Task Create_with_a_taken_key_fails_with_identitys_error()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new RolesMdRepo(harness.RoleManager);
        Assert.True((await repo.Create(NewRole("Editor", "რედაქტორი", 50))).IsSuccess);

        Result result = await repo.Create(NewRole("Editor", "მეორე", 60));

        Assert.True(result.IsFailure);
        Assert.Contains(Errors(result), e => e.Code == "DuplicateRoleName");
        Assert.Single(harness.Db.Context.Roles);
    }

    [Fact]
    public async Task Update_succeeds_and_stores_the_new_fields()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new RolesMdRepo(harness.RoleManager);
        Role role = NewRole("Editor", "რედაქტორი", 50);
        Assert.True((await repo.Create(role)).IsSuccess);

        Result result = await repo.Update(role.RolId, NewRole("Editor", "ავტორი", 40));

        Assert.True(result.IsSuccess);
        Role stored = harness.Db.Context.Roles.Single();
        Assert.Equal("Editor", stored.RolKey);
        Assert.Equal("ავტორი", stored.RolName);
        Assert.Equal(40, stored.RolLevel);
    }

    [Fact]
    public async Task Update_fails_when_the_store_cannot_update_the_role()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new RolesMdRepo(harness.RoleManager);
        Role role = NewRole("Editor", "რედაქტორი", 50);
        Assert.True((await repo.Create(role)).IsSuccess);
        harness.ForbidRoleUpdates();

        Result result = await repo.Update(role.RolId, NewRole("Editor", "ავტორი", 40));

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
    }

    [Fact]
    public async Task Delete_of_an_existing_role_succeeds_and_removes_the_row()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new RolesMdRepo(harness.RoleManager);
        Role role = NewRole("Editor", "რედაქტორი", 50);
        Assert.True((await repo.Create(role)).IsSuccess);

        Result result = await repo.Delete(role.RolId);

        Assert.True(result.IsSuccess);
        Assert.Empty(harness.Db.Context.Roles);
    }

    [Fact]
    public async Task Delete_fails_when_the_store_cannot_delete_the_role()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new RolesMdRepo(harness.RoleManager);
        Role role = NewRole("Editor", "რედაქტორი", 50);
        Assert.True((await repo.Create(role)).IsSuccess);
        harness.ForbidRoleDeletes();

        Result result = await repo.Delete(role.RolId);

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
    }

    [Fact]
    public async Task Delete_of_a_missing_role_answers_that_the_role_cannot_be_found()
    {
        using var harness = new IdentityManagersHarness();
        var repo = new RolesMdRepo(harness.RoleManager);

        Result result = await repo.Delete(12345);

        Assert.Equal(MasterDataApiErrors.CannotFindRole, result.Error);
    }

    private static Error[] Errors(Result result)
    {
        return Assert.IsType<ValidationError>(result.Error).Errors;
    }

    private static Role NewRole(string key, string name, int level)
    {
        return new Role { RolKey = key, RolName = name, RolLevel = level, RolNormalizedKey = key.ToUpperInvariant() };
    }
}
