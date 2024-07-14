using CarcassDom;
using CarcassRights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;

namespace BackendCarcassApi.Filters;

public class UserClaimRightsFilter : IEndpointFilter
{
    private readonly string _claimName;
    private readonly ILogger<UserClaimRightsFilter> _logger;
    private readonly IUserRightsRepository _repo;

    protected UserClaimRightsFilter(string claimName, IUserRightsRepository repo, ILogger<UserClaimRightsFilter> logger)
    {
        _claimName = claimName;
        _repo = repo;
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var userName = context.HttpContext.User.Identity?.Name;
        if (userName == null)
            return Results.BadRequest(new[] { RightsApiErrors.UserNotIdentified });

        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
        RightsDeterminer rightsDeterminer = new(_repo, _logger);
        var result =
            await rightsDeterminer.CheckUserRightToClaim(context.HttpContext.User.Claims, _claimName,
                CancellationToken.None);
        if (result.IsT1)
            return Results.BadRequest(result.AsT1);
        if (!result.AsT0)
            //თუ არა დაბრუნდეს შეცდომა
            return Results.BadRequest(new[] { RightsApiErrors.InsufficientRights });

        return await next(context);
    }
}