using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassDom;
using CarcassIdentity;
using CarcassRights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackendCarcassApi.Filters;

public class UserMenuRightsFilter : IEndpointFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserMenuRightsFilter> _logger;
    private readonly IEnumerable<string> _menuNames;
    private readonly IUserRightsRepository _repo;

    protected UserMenuRightsFilter(IEnumerable<string> menuNames, IUserRightsRepository repo,
        ILogger<UserMenuRightsFilter> logger, ICurrentUser currentUser)
    {
        _menuNames = menuNames;
        _repo = repo;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var userName = context.HttpContext.User.Identity?.Name;
        if (userName == null)
            return Results.BadRequest(new[] { RightsApiErrors.UserNotIdentified });

        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
        RightsDeterminer rightsDeterminer = new(_repo, _logger, _currentUser);
        var result = await rightsDeterminer.HasUserRightRole(_menuNames, CancellationToken.None);
        if (result.IsT1)
            return Results.BadRequest(result.AsT1);
        if (!result.AsT0)
            //თუ არა დაბრუნდეს შეცდომა
            return Results.BadRequest(new[] { RightsApiErrors.InsufficientRights });

        return await next(context);
    }
}