using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Rights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Api.Filters;

public sealed class UserTableRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly IDatabaseAbstraction _databaseAbstraction;
    private readonly ILogger<UserTableRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserTableRightsFilter(IUserRightsRepository repo, ILogger<UserTableRightsFilter> logger,
        ICurrentUser currentUser, IDatabaseAbstraction databaseAbstraction)
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
        _databaseAbstraction = databaseAbstraction;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        RouteValueDictionary routeData = context.HttpContext.Request.RouteValues;
        routeData.TryGetValue("tableName", out object? tableName);
        string strTableName = tableName?.ToString() ?? string.Empty;

        var rightsDeterminer = new RightsDeterminer(_repo, _logger, _currentUser, _databaseAbstraction);
        Result checkTableRightsResult = await rightsDeterminer.CheckTableRights(_currentUser.Name,
            context.HttpContext.Request.Method, new TableKeyName { TableName = strTableName }, CancellationToken.None);

        if (checkTableRightsResult.IsFailure)
        {
            return Results.BadRequest(checkTableRightsResult.Error.ToErrorOmdArray());
        }

        return await next(context);
    }
}
