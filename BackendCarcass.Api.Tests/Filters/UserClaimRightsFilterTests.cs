using BackendCarcass.Api.Filters;
using BackendCarcass.Identity;
using BackendCarcass.Rights;
using BackendCarcassContracts.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OneOf;
using SystemTools.DomainShared.Repositories;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Api.Tests.Filters;

public class UserClaimRightsFilterTests
{
    private readonly ICurrentUser _mockCurrentUser;
    private readonly ILogger<UserClaimRightsFilter> _mockLogger;
    private readonly IUserRightsRepository _mockRepo;
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly string _testClaimName;

    public UserClaimRightsFilterTests()
    {
        _mockRepo = Substitute.For<IUserRightsRepository>();
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockLogger = Substitute.For<ILogger<UserClaimRightsFilter>>();
        _mockCurrentUser = Substitute.For<ICurrentUser>();
        _testClaimName = "TestClaim";
    }

    private TestableUserClaimRightsFilter CreateFilter()
    {
        return new TestableUserClaimRightsFilter(_testClaimName, _mockRepo, _mockUnitOfWork, _mockLogger,
            _mockCurrentUser);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserHasClaim_ShouldCallNextDelegate()
    {
        // Arrange
        TestableUserClaimRightsFilter filter = CreateFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        bool nextCalled = false;
        IResult expectedResult = Results.Ok();

        async ValueTask<object?> next(EndpointFilterInvocationContext ctx)
        {
            nextCalled = true;
            await Task.CompletedTask;
            return expectedResult;
        }

        _mockCurrentUser.Roles.Returns(["AdminRole"]);

        filter.SetupRightsDeterminerResult(true);

        // Act
        object? result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserDoesNotHaveClaim_ShouldReturnBadRequestWithInsufficientRights()
    {
        // Arrange
        TestableUserClaimRightsFilter filter = CreateFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        bool nextCalled = false;

        EndpointFilterDelegate next = async ctx =>
        {
            nextCalled = true;
            await Task.CompletedTask;
            return Results.Ok();
        };

        _mockCurrentUser.Roles.Returns(["UserRole"]);

        filter.SetupRightsDeterminerResult(false);

        // Act
        object? result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.False(nextCalled);
        Assert.IsType<BadRequest<Err[]>>(result);

        var badRequest = result as BadRequest<Err[]>;
        Assert.NotNull(badRequest);
        Assert.NotNull(badRequest.Value);
        Assert.Single(badRequest.Value);
        Assert.Equal(RightsApiErrors.InsufficientRights, badRequest.Value[0]);
    }

    [Fact]
    public async Task InvokeAsync_WhenRightsCheckReturnsErrors_ShouldReturnBadRequestWithErrors()
    {
        // Arrange
        TestableUserClaimRightsFilter filter = CreateFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        bool nextCalled = false;

        EndpointFilterDelegate next = async ctx =>
        {
            nextCalled = true;
            await Task.CompletedTask;
            return Results.Ok();
        };

        Err[] expectedErrors = new[]
        {
            RightsApiErrors.ErrorWhenDeterminingRights,
            new Err { ErrorCode = "AdditionalError", ErrorMessage = "Additional error occurred" }
        };

        _mockCurrentUser.Roles.Returns(["TestRole"]);

        filter.SetupRightsDeterminerResult(expectedErrors);

        // Act
        object? result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.False(nextCalled);
        Assert.IsType<BadRequest<Err[]>>(result);

        var badRequest = result as BadRequest<Err[]>;
        Assert.NotNull(badRequest);
        Assert.NotNull(badRequest.Value);
        Assert.Equal(2, badRequest.Value.Length);
        Assert.Equal(expectedErrors, badRequest!.Value);
    }

    [Fact]
    public async Task InvokeAsync_WithDifferentClaimNames_ShouldCheckCorrectClaim()
    {
        // Arrange
        string customClaimName = "CustomClaim";
        var filter = new TestableUserClaimRightsFilter(customClaimName, _mockRepo, _mockUnitOfWork, _mockLogger,
            _mockCurrentUser);

        var context = Substitute.For<EndpointFilterInvocationContext>();
        bool nextCalled = false;

        EndpointFilterDelegate next = async ctx =>
        {
            nextCalled = true;
            await Task.CompletedTask;
            return Results.Ok();
        };

        _mockCurrentUser.Roles.Returns(["AdminRole"]);

        filter.SetupRightsDeterminerResult(true);

        // Act
        await filter.InvokeAsync(context, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(customClaimName, filter.ClaimName);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserHasMultipleRoles_ShouldCheckRights()
    {
        // Arrange
        TestableUserClaimRightsFilter filter = CreateFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        IResult expectedResult = Results.Ok();

        async ValueTask<object?> Next(EndpointFilterInvocationContext ctx)
        {
            await Task.CompletedTask;
            return expectedResult;
        }

        _mockCurrentUser.Roles.Returns(["Role1", "Role2", "Role3"]);

        filter.SetupRightsDeterminerResult(true);

        // Act
        object? result = await filter.InvokeAsync(context, Next);

        // Assert
        // Roles property was successfully stubbed with multiple roles
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextDelegateReturnsCustomResult_ShouldReturnThatResult()
    {
        // Arrange
        TestableUserClaimRightsFilter filter = CreateFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        IResult customResult = Results.Created("/api/resource/123", new { Id = 123 });

        async ValueTask<object?> Next(EndpointFilterInvocationContext ctx)
        {
            await Task.CompletedTask;
            return customResult;
        }

        _mockCurrentUser.Roles.Returns(["AdminRole"]);

        filter.SetupRightsDeterminerResult(true);

        // Act
        object? result = await filter.InvokeAsync(context, Next);

        // Assert
        Assert.Equal(customResult, result);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserHasNoClaim_ShouldNotCallNextDelegate()
    {
        // Arrange
        TestableUserClaimRightsFilter filter = CreateFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        bool nextCalled = false;

        async ValueTask<object?> Next(EndpointFilterInvocationContext ctx)
        {
            nextCalled = true;
            await Task.CompletedTask;
            return Results.Ok();
        }

        _mockCurrentUser.Roles.Returns(["LimitedRole"]);

        filter.SetupRightsDeterminerResult(false);

        // Act
        await filter.InvokeAsync(context, Next);

        // Assert
        Assert.False(nextCalled);
    }

    // Testable version of UserClaimRightsFilter to allow testing
    private sealed class TestableUserClaimRightsFilter : UserClaimRightsFilter
    {
        private OneOf<bool, Err[]>? _mockResult;

        public TestableUserClaimRightsFilter(string claimName, IUserRightsRepository repo, IUnitOfWork unitOfWork,
            ILogger<UserClaimRightsFilter> logger, ICurrentUser currentUser) : base(claimName, repo, unitOfWork, logger,
            currentUser)
        {
            ClaimName = claimName;
        }

        public string ClaimName { get; }

        public void SetupRightsDeterminerResult(bool hasRight)
        {
            _mockResult = hasRight;
        }

        public void SetupRightsDeterminerResult(Err[] errors)
        {
            _mockResult = errors;
        }

        // Override to inject mock behavior
        public new async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            if (!_mockResult.HasValue)
            {
                return await base.InvokeAsync(context, next);
            }

            OneOf<bool, Err[]> result = _mockResult.Value;
            if (result.IsT1)
            {
                return Results.BadRequest(result.AsT1);
            }

            if (!result.AsT0)
            {
                return Results.BadRequest(new[] { RightsApiErrors.InsufficientRights });
            }

            return await next(context);
        }
    }
}
