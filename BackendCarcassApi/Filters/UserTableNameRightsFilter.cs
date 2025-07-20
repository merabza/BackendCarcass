using System.Threading;
using System.Threading.Tasks;
using CarcassDom;
using CarcassIdentity;
using CarcassRights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public /*open*/ class UserTableNameRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserMenuRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;
    private readonly string[] _tableKeys;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected UserTableNameRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger,
        string[] tableKeys, ICurrentUser currentUser)
    {
        _repo = repo;
        _logger = logger;
        _tableKeys = tableKeys;
        _currentUser = currentUser;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var tableKey in _tableKeys)
        {
            var rightsDeterminer = new RightsDeterminer(_repo, _logger, _currentUser);
            var checkTableRightsResult = await rightsDeterminer.CheckTableRights(_currentUser.Name,
                context.HttpContext.Request.Method, new TableKeyName { TableKey = tableKey }, CancellationToken.None);
            if (checkTableRightsResult.IsSome)
                return checkTableRightsResult;
        }

        return await next(context);
    }
}