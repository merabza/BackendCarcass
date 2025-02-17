using System.Threading;
using System.Threading.Tasks;
using CarcassDom;
using CarcassIdentity;
using CarcassRights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public class UserTableRightsFilter : IEndpointFilter
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

        RightsDeterminer rightsDeterminer = new(_repo, _logger, _currentUser);
        var result = await rightsDeterminer.CheckTableRights(context.HttpContext.User.Identity?.Name,
            context.HttpContext.Request.Method, new TableKeyName { TableName = strTableName }, CancellationToken.None);

        if (result != null)
            return result;

        return await next(context);
    }
}