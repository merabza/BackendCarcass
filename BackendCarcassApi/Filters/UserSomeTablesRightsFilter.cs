//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using BackendCarcassContracts.Errors;
//using CarcassDom;
//using CarcassIdentity;
//using CarcassRights;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;

//// ReSharper disable ReplaceWithPrimaryConstructorParameter

//namespace BackendCarcassApi.Filters;

////შემოწმდეს, ყველა მოწოდებულ ცხრილზე ნახვის უფლება აქვს თუ არა მომხმარებელს
////თუ რომელიმეზე არ აქვს, დაბრუნდეს შეცდომა

//public class UserSomeTablesRightsFilter : IEndpointFilter
//{
//    private readonly ICurrentUser _currentUser;
//    private readonly ILogger<UserMenuRightsFilter> _logger;
//    private readonly IUserRightsRepository _repo;

//    // ReSharper disable once ConvertToPrimaryConstructor
//    public UserSomeTablesRightsFilter(IUserRightsRepository repo, ILogger<UserMenuRightsFilter> logger,
//        ICurrentUser currentUser)
//    {
//        _logger = logger;
//        _currentUser = currentUser;
//        _repo = repo;
//    }

//    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
//    {
//        var userName = context.HttpContext.User.Identity?.Name;
//        if (userName == null)
//            return Results.BadRequest(new[] { RightsApiErrors.UserNotIdentified });

//        var reqQuery = context.HttpContext.Request.Query["tables"];
//        var tableKeysNames = reqQuery.Distinct().Select(x => new TableKeyName { TableName = x }).ToList();

//        if (tableKeysNames.Count == 0)
//            return Results.BadRequest(new[] { RightsApiErrors.TableNamesListNotIdentified });

//        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
//        RightsDeterminer rightsDeterminer = new(_repo, _logger, _currentUser);
//        var result = await rightsDeterminer.CheckTableListViewRight(tableKeysNames, CancellationToken.None);
//        if (result.IsT1)
//            return Results.BadRequest(result.AsT1);
//        if (!result.AsT0)
//            //თუ არა დაბრუნდეს შეცდომა
//            return Results.BadRequest(new[] { RightsApiErrors.InsufficientRights });

//        return await next(context);
//    }
//}