using System.Threading;
using System.Threading.Tasks;
using CarcassIdentity;
using CarcassRights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public sealed class UserTableRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserMenuRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserTableRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var routeData = context.HttpContext.Request.RouteValues;
        routeData.TryGetValue("tableName", out var tableName);
        var strTableName = tableName?.ToString() ?? string.Empty;

        var rightsDeterminer = new RightsDeterminer(_repo, _logger, _currentUser);
        var checkTableRightsResult = await rightsDeterminer.CheckTableRights(_currentUser.Name,
            context.HttpContext.Request.Method, new TableKeyName { TableName = strTableName }, CancellationToken.None);

        if (checkTableRightsResult.IsSome)
            return checkTableRightsResult;

        return await next(context);
    }
}