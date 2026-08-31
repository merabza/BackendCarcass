using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Rights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Api.Filters;

public /*open*/ class UserTableNameRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly IDatabaseAbstraction _databaseAbstraction;
    private readonly ILogger _logger;
    private readonly IUserRightsRepository _repo;
    private readonly string[] _tableKeys;

    protected UserTableNameRightsFilter(ILogger logger, string[] tableKeys, ICurrentUser currentUser,
        IUserRightsRepository repo, IDatabaseAbstraction databaseAbstraction)
    {
        _logger = logger;
        _tableKeys = tableKeys;
        _currentUser = currentUser;
        _databaseAbstraction = databaseAbstraction;
        _repo = repo;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (string tableKey in _tableKeys)
        {
            var rightsDeterminer = new RightsDeterminer(_repo, _logger, _currentUser, _databaseAbstraction);
            Result checkTableRightsResult = await rightsDeterminer.CheckTableRights(_currentUser.Name,
                context.HttpContext.Request.Method, new TableKeyName { TableName = tableKey }, CancellationToken.None);
            if (checkTableRightsResult.IsFailure)
            {
                return Result.Failure(checkTableRightsResult.Error);
            }
        }

        return await next(context);
    }
}
