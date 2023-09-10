using System.Threading;
using System.Threading.Tasks;
using CarcassRights;
using CarcassRightsDom;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public class UserTableRightsFilter : IEndpointFilter
{
    private readonly ILogger<UserMenuRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;

    public UserTableRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var routeData = context.HttpContext.Request.RouteValues;
        routeData.TryGetValue("tableName", out var tableName);
        var strTableName = tableName?.ToString() ?? string.Empty;

        //var tableChecker = new TableChecker(_repo, _logger, context, strTableName);

        RightsDeterminer rightsDeterminer = new(_repo, _logger);
        var result = await rightsDeterminer.CheckTableRights(context.HttpContext.User.Identity?.Name,
            context.HttpContext.Request.Method, new TableKeyName { TableName = strTableName },
            context.HttpContext.User.Claims, CancellationToken.None);


        //var result = await tableChecker.CheckTableRights();
        if (result != null)
            return result;

        return await next(context);
    }
}