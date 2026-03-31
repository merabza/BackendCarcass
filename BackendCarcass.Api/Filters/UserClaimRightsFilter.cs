using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.Rights;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Api.Filters;

public /*open*/ class UserClaimRightsFilter : IEndpointFilter
{
    private readonly string _claimName;
    private readonly ICurrentUser _currentUser;
    private readonly IDatabaseAbstraction _databaseAbstraction;
    private readonly ILogger<UserClaimRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;

    protected UserClaimRightsFilter(string claimName, IUserRightsRepository repo, ILogger<UserClaimRightsFilter> logger,
        ICurrentUser currentUser, IDatabaseAbstraction databaseAbstraction)
    {
        _claimName = claimName;
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
        _databaseAbstraction = databaseAbstraction;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
        var rightsDeterminer = new RightsDeterminer(_repo, _logger, _currentUser, _databaseAbstraction);
        OneOf<bool, Error[]> result = await rightsDeterminer.CheckUserRightToClaim(_claimName, CancellationToken.None);
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
