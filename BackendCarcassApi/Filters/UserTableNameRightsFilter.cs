using System.Threading;
using System.Threading.Tasks;
using CarcassIdentity;
using CarcassRights;
using DomainShared.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public /*open*/ class UserTableNameRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserMenuRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;
    private readonly string[] _tableKeys;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected UserTableNameRightsFilter(ILogger<UserMenuRightsFilter> logger, string[] tableKeys,
        ICurrentUser currentUser, IUserRightsRepository repo, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _tableKeys = tableKeys;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _repo = repo;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var tableKey in _tableKeys)
        {
            var rightsDeterminer = new RightsDeterminer(_repo, _unitOfWork, _logger, _currentUser);
            var checkTableRightsResult = await rightsDeterminer.CheckTableRights(_currentUser.Name,
                context.HttpContext.Request.Method, new TableKeyName { TableKey = tableKey }, CancellationToken.None);
            if (checkTableRightsResult.IsSome)
                return checkTableRightsResult;
        }

        return await next(context);
    }
}