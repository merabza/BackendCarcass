using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Tests.TestInfrastructure;
using BackendCarcass.Application.UserRights.ChangeProfile;
using BackendCarcass.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using SystemTools.SharedKernel;
using Xunit;

namespace BackendCarcass.Application.Tests.UserRights;

// PUT api/v1/userrights/changeprofile stored the new names but kept the old FullName, which the users list shows
// (BackendCarcass#4)
public sealed class ChangeProfileCommandHandlerTests
{
    [Theory]
    [InlineData("ნინო", "User", "ნინო User")]
    [InlineData("Test", "ბერიძე", "Test ბერიძე")]
    public async Task Changing_a_name_stores_the_new_full_name(string firstName, string lastName,
        string expectedFullName)
    {
        using var harness = new IdentityManagersHarness();
        User user = harness.Db.AddUser();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.Name).Returns("tester");
        var handler = new ChangeProfileCommandHandler(harness.UserManager, currentUser.Object);

        Result result = await handler.Handle(new ChangeProfileRequestCommand
        {
            Userid = user.UsrId,
            UserName = "tester",
            Email = "tester@example.com",
            FirstName = firstName,
            LastName = lastName
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        User stored = await harness.Db.Context.Users.AsNoTracking().SingleAsync();
        Assert.Equal(firstName, stored.FirstName);
        Assert.Equal(lastName, stored.LastName);
        Assert.Equal(expectedFullName, stored.FullName);
    }
}
