using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Api.Filters;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Rights;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using Xunit;

namespace BackendCarcass.Api.Tests.Filters;

public sealed class UserClaimRightsFilterTests
{
    private const string ClaimName = "SeeAllIssues";

    private sealed class TestClaimRightsFilter(
        IUserRightsRepository repo,
        ILogger<UserClaimRightsFilter> logger,
        ICurrentUser currentUser,
        IDatabaseAbstraction databaseAbstraction)
        : UserClaimRightsFilter(ClaimName, repo, logger, currentUser, databaseAbstraction);

    private static async Task<object?> Invoke(RightsFilterTestContext context)
    {
        var filter = new TestClaimRightsFilter(context.Repository.Object,
            new Mock<ILogger<UserClaimRightsFilter>>().Object, context.CurrentUser.Object,
            context.DatabaseAbstraction.Object);
        return await filter.InvokeAsync(RightsFilterTestContext.InvocationContext(), context.Next);
    }

    private static void SetupClaimRight(RightsFilterTestContext context, string role, bool hasRight)
    {
        context.Repository.Setup(r => r.CheckRight(RightsFilterTestContext.DataTypeId, role,
                RightsFilterTestContext.DataTypeId, ClaimName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasRight);
    }

    [Fact]
    public async Task InvokeAsync_UserRoleHasClaim_CallsNext()
    {
        // Arrange
        var context = new RightsFilterTestContext("Admin");
        SetupClaimRight(context, "Admin", true);

        // Act
        object? result = await Invoke(context);

        // Assert
        Assert.Same(RightsFilterTestContext.NextResult, result);
        Assert.Equal(1, context.NextCalls);
    }

    [Fact]
    public async Task InvokeAsync_NoRoleHasClaim_Returns403WithInsufficientRights()
    {
        // Arrange
        var context = new RightsFilterTestContext("Guest", "Manager");
        SetupClaimRight(context, "Guest", false);
        SetupClaimRight(context, "Manager", false);

        // Act
        object? result = await Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Error[] errors = Assert.IsType<Error[]>(Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Equal(RightsApiErrors.InsufficientRights.Code, Assert.Single(errors).Code);
        Assert.Equal(0, context.NextCalls);
    }

    [Fact]
    public async Task InvokeAsync_RightsCannotBeDetermined_Returns400WithTheError()
    {
        // Arrange
        var context = new RightsFilterTestContext("Admin");
        context.Repository.Setup(r => r.GetDataTypeIdByKey(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        // Act
        object? result = await Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Error error = Assert.IsType<Error>(Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Equal(RightsApiErrors.ErrorWhenDeterminingRights.Code, error.Code);
        Assert.Equal(0, context.NextCalls);
    }
}
