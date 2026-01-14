using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassIdentity;
using CarcassRights;
using DomainShared.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public /*open*/ class UserMenuRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserMenuRightsFilter> _logger;
    private readonly IEnumerable<string> _menuNames;
    private readonly IUserRightsRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    protected UserMenuRightsFilter(IEnumerable<string> menuNames, IUserRightsRepository repo, IUnitOfWork unitOfWork,
        ILogger<UserMenuRightsFilter> logger, ICurrentUser currentUser)
    {
        _menuNames = menuNames;
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
        var rightsDeterminer = new RightsDeterminer(_repo, _unitOfWork, _logger, _currentUser);
        var result = await rightsDeterminer.HasUserRightRole(_menuNames, CancellationToken.None);
        if (result.IsT1)
        {
            return Results.BadRequest(result.AsT1);
        }

        if (!result.AsT0)
            //თუ არა დაბრუნდეს შეცდომა
        {
            return Results.BadRequest(new[] { RightsApiErrors.InsufficientRights });
        }

        return await next(context);
    }
}
