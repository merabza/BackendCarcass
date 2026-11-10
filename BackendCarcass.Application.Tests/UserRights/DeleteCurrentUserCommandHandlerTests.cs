using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Application.UserRights.DeleteCurrentUser;
using BackendCarcassShared.Contracts.Errors;
using Moq;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.UserRights;

// DELETE api/v1/userrights/deletecurrentuser/{userName} deleted the user but answered 400 (BackendCarcass#3)
public sealed class DeleteCurrentUserCommandHandlerTests
{
    [Fact]
    public async Task Deleting_the_current_user_succeeds_and_removes_the_row()
    {
        using var harness = new IdentityManagersHarness();
        harness.Db.AddUser();
        DeleteCurrentUserCommandHandler handler = Handler(harness, "tester");

        Result result = await handler.Handle(new DeleteCurrentUserRequestCommand { UserName = "tester" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(harness.Db.Context.Users);
    }

    [Fact]
    public async Task A_failed_delete_answers_that_the_user_could_not_be_deleted()
    {
        using var harness = new IdentityManagersHarness();
        harness.Db.AddUser();
        harness.ForbidUserDeletes();
        DeleteCurrentUserCommandHandler handler = Handler(harness, "tester");

        Result result = await handler.Handle(new DeleteCurrentUserRequestCommand { UserName = "tester" },
            CancellationToken.None);

        Assert.Equal(UserRightsErrors.DeletionErrorUserCouldNotBeDeleted, result.Error);
    }

    private static DeleteCurrentUserCommandHandler Handler(IdentityManagersHarness harness, string currentUserName)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.Name).Returns(currentUserName);
        return new DeleteCurrentUserCommandHandler(harness.UserManager, currentUser.Object);
    }
}
