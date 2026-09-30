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

public sealed class UserMenuRightsFilterTests
{
    private const string MenuKey = "studentContracts";

    private sealed class TestMenuRightsFilter(
        IUserRightsRepository repo,
        ILogger<UserMenuRightsFilter> logger,
        ICurrentUser currentUser,
        IDatabaseAbstraction databaseAbstraction)
        : UserMenuRightsFilter([MenuKey], repo, logger, currentUser, databaseAbstraction);

    private static async Task<object?> Invoke(RightsFilterTestContext context)
    {
        var filter = new TestMenuRightsFilter(context.Repository.Object, new Mock<ILogger<UserMenuRightsFilter>>().Object,
            context.CurrentUser.Object, context.DatabaseAbstraction.Object);
        return await filter.InvokeAsync(RightsFilterTestContext.InvocationContext(), context.Next);
    }

    private static void SetupMenuRight(RightsFilterTestContext context, string role, bool hasRight)
    {
        context.Repository.Setup(r => r.CheckMenuRight(RightsFilterTestContext.DataTypeId, role,
                RightsFilterTestContext.DataTypeId, RightsFilterTestContext.DataTypeId, MenuKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasRight);
    }

    [Fact]
    public async Task InvokeAsync_UserRoleHasMenuRight_CallsNext()
    {
        // Arrange
        var context = new RightsFilterTestContext("Manager");
        SetupMenuRight(context, "Manager", true);

        // Act
        object? result = await Invoke(context);

        // Assert
        Assert.Same(RightsFilterTestContext.NextResult, result);
        Assert.Equal(1, context.NextCalls);
    }

    [Fact]
    public async Task InvokeAsync_SecondRoleHasMenuRight_CallsNext()
    {
        // Arrange
        var context = new RightsFilterTestContext("Guest", "Manager");
        SetupMenuRight(context, "Guest", false);
        SetupMenuRight(context, "Manager", true);

        // Act
        object? result = await Invoke(context);

        // Assert
        Assert.Same(RightsFilterTestContext.NextResult, result);
    }

    [Fact]
    public async Task InvokeAsync_NoRoleHasMenuRight_Returns403WithInsufficientRights()
    {
        // Arrange
        var context = new RightsFilterTestContext("Guest");
        SetupMenuRight(context, "Guest", false);

        // Act
        object? result = await Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Error[] errors = Assert.IsType<Error[]>(Assert.IsAssignableFrom<IValueHttpResult>(result).Value);
        Assert.Equal(RightsApiErrors.InsufficientRights.Code, Assert.Single(errors).Code);
        Assert.Equal(0, context.NextCalls);
    }

    [Fact]
    public async Task InvokeAsync_UserWithoutRoles_Returns403()
    {
        // Arrange
        var context = new RightsFilterTestContext();

        // Act
        object? result = await Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(0, context.NextCalls);
    }

    [Fact]
    public async Task InvokeAsync_RightsCannotBeDetermined_Returns400WithTheError()
    {
        // Arrange
        var context = new RightsFilterTestContext("Manager");
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
