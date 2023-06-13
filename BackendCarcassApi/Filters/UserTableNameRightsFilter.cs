using System.Threading.Tasks;
using CarcassRights;
using CarcassRightsDom;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public class UserTableNameRightsFilter : IEndpointFilter
{
    private readonly ILogger<UserMenuRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;
    private readonly string[] _tableKeys;

    public UserTableNameRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger,
        string[] tableKeys)
    {
        _repo = repo;
        _logger = logger;
        _tableKeys = tableKeys;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var tableKey in _tableKeys)
        {
            RightsDeterminer rightsDeterminer = new(_repo, _logger);
            var result = await rightsDeterminer.CheckTableRights(context.HttpContext.User.Identity?.Name,
                context.HttpContext.Request.Method, new TableKeyName { TableKey = tableKey },
                context.HttpContext.User.Claims);
            if (result != null)
                return result;
        }


        return await next(context);
    }
}