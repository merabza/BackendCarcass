using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassDom;
using CarcassIdentity;
using CarcassRights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public class UserClaimRightsFilter : IEndpointFilter
{
    private readonly string _claimName;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserClaimRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;

    protected UserClaimRightsFilter(string claimName, IUserRightsRepository repo, ILogger<UserClaimRightsFilter> logger,
        ICurrentUser currentUser)
    {
        _claimName = claimName;
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
        RightsDeterminer rightsDeterminer = new(_repo, _logger, _currentUser);
        var result = await rightsDeterminer.CheckUserRightToClaim(_claimName, CancellationToken.None);
        if (result.IsT1)
            return Results.BadRequest(result.AsT1);
        if (!result.AsT0)
            //თუ არა დაბრუნდეს შეცდომა
            return Results.BadRequest(new[] { RightsApiErrors.InsufficientRights });

        return await next(context);
    }
}