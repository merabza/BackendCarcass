using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.Rights;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SystemTools.DomainShared.Repositories;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Api.Filters;

public sealed class UserTableRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserTableRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserTableRightsFilter(IUserRightsRepository repo, IUnitOfWork unitOfWork,
        ILogger<UserTableRightsFilter> logger, ICurrentUser currentUser)
    {
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        RouteValueDictionary routeData = context.HttpContext.Request.RouteValues;
        routeData.TryGetValue("tableName", out object? tableName);
        string strTableName = tableName?.ToString() ?? string.Empty;

        var rightsDeterminer = new RightsDeterminer(_repo, _unitOfWork, _logger, _currentUser);
        Option<BadRequest<Err[]>> checkTableRightsResult = await rightsDeterminer.CheckTableRights(_currentUser.Name,
            context.HttpContext.Request.Method, new TableKeyName { TableName = strTableName }, CancellationToken.None);

        if (checkTableRightsResult.IsSome)
        {
            return checkTableRightsResult;
        }

        return await next(context);
    }
}
