//using CarcassContracts.ErrorModels;
//using CarcassRights;
//using CarcassRightsDom;
//using LanguageExt;
//using Microsoft.AspNetCore.Http;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

//namespace ServerCarcassMini.Filters;

//public class TableChecker
//{
//    private readonly IUserRightsRepository _repo;
//    private readonly ILogger _logger;
//    private readonly EndpointFilterInvocationContext _context;
//    private readonly string _tableKey;

//    public TableChecker(IUserRightsRepository repo, ILogger logger, EndpointFilterInvocationContext context, string tableKey)
//    {
//        _repo = repo;
//        _logger = logger;
//        _context = context;
//        _tableKey = tableKey;
//    }

//    public async Task<IResult?> CheckTableRights()
//    {
//        //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს _claimName-ის შესაბამისი სპეციალური უფლება
//        RightsDeterminer rightsDeterminer = new(_repo, _logger);
//        var method = _context.HttpContext.Request.Method;

//        return await rightsDeterminer.CheckTableRights(_context.HttpContext.User.Identity?.Name, method, _tableKey, _context.HttpContext.User.Claims);

//        var userName = _context.HttpContext.User.Identity?.Name;
//        if (userName == null)
//            return Results.BadRequest(new[] { RightsApiErrors.UserNotIdentified });

//        if (_tableKey == string.Empty)
//            return Results.BadRequest(new[] { RightsApiErrors.TableNameNotIdentified });

//        var result = method == HttpMethods.Get
//            ? await rightsDeterminer.CheckViewRightByTableKey(_tableKey, _context.HttpContext.User.Claims)
//            : await rightsDeterminer.CheckCrudRightByTableKey(_tableKey, _context.HttpContext.User.Claims,
//                GetCrudType(method));
//        if (result.IsT1)
//            return Results.BadRequest(result.AsT1);

//        //თუ არა დაბრუნდეს შეცდომა
//        return !result.AsT0 ? Results.BadRequest(new[] { RightsApiErrors.InsufficientRights }) : null;
//    }

//    private static Option<ECrudOperationType> GetCrudType(string method)
//    {
//        if (method == HttpMethods.Post)
//            return ECrudOperationType.Create;
//        if (method == HttpMethods.Put)
//            return ECrudOperationType.Update;
//        return method == HttpMethods.Delete ? ECrudOperationType.Delete : new Option<ECrudOperationType>();
//    }

//}

