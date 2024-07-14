using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassApi.Filters;
using BackendCarcassApi.Handlers.UserRights;
using BackendCarcassApi.Mappers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using BackendCarcassContracts.V1.Requests;
using BackendCarcassContracts.V1.Routes;
using WebInstallers;

namespace BackendCarcassApi.Endpoints.V1;

//კონტროლერი -> რომელიც დანიშნულებაცაა მომხმარებლის პირადი ინფორმაციის მომსახურება
//აქ არსებული მოქმედებები დამატებით უფლებებს არ ითხოვს
//მთავარია ავტორიზაცია ჰქონდეს გავლილი მომხმარებელს
//[Authorize]
//[ApiController]
//[Route("api/[controller]")]

// ReSharper disable once UnusedType.Global
public sealed class UserRightsEndpoints : IInstaller
{
    public int InstallPriority => 70;
    public int ServiceUsePriority => 70;

    public void InstallServices(WebApplicationBuilder builder, string[] args, Dictionary<string, string> parameters)
    {
    }

    public void UseServices(WebApplication app)
    {
        //Console.WriteLine("UserRightsEndpoints.UseServices Started");
        var group = app.MapGroup(CarcassApiRoutes.UserRights.UserRightsBase).RequireAuthorization()
            .AddEndpointFilter<UserNameFilter>();

        group.MapGet(CarcassApiRoutes.UserRights.IsCurrentUserValid, IsCurrentUserValid);
        group.MapPut(CarcassApiRoutes.UserRights.ChangeProfile, ChangeProfile);
        group.MapPut(CarcassApiRoutes.UserRights.ChangePassword, ChangePassword);
        group.MapDelete(CarcassApiRoutes.UserRights.DeleteCurrentUser, DeleteCurrentUser);
        group.MapGet(CarcassApiRoutes.UserRights.MainMenu, MainMenu);
        //Console.WriteLine("UserRightsEndpoints.UseServices Finished");
    }
    //private readonly IMasterDataRepository _mdRepo;
    //private readonly UserManager<AppUser> _userManager;
    //private readonly ILogger<UserRightsController> _logger;

    ////private int CurrentUserId => int.TryParse(HttpContext.User.Identity.Name, out int userId) ? userId : 0;
    //private string? CurrentUserName => HttpContext.User.Claims.SingleOrDefault(so => so.Type == ClaimTypes.Name)?.Value;
    //private static string? CurrentUserName(HttpRequest request)
    //{
    //    return request.HttpContext.User.Claims.SingleOrDefault(so => so.Type == ClaimTypes.Name)?.Value;
    //}


    //public UserRightsController(IMasterDataRepository mdRepo, UserManager<AppUser> userManager,
    //    ILogger<UserRightsController> logger)
    //{
    //    _mdRepo = mdRepo;
    //    _userManager = userManager;
    //    _logger = logger;
    //}

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის შემოწმება
    //შემავალი ინფორმაცია -> არა
    //უფლება -> ნებისმიერი
    //მოქმედება -> თუ ამ მეთოდამდე მოვიდა კოდი, ეს ნიშნავს, რომ მომხმარებელს ავტორიზაცია აქვს გავლილი
    //   ამიტომ მეთოდი ყოველთვის აბრუნებს Ok()-ს
    // GET api/v1/userrights/iscurrentuservalid
    private static IResult IsCurrentUserValid()
    {
        return Results.Ok();
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის შესახებ ინფორმაციის ცვლილება
    //შემავალი ინფორმაცია -> ChangeProfileModel კლასის ობიექტი
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული ინფორმაციის ვალიდურობა და ხდება პროფაილში ცვლილებების დაფიქსირება
    // GET api/v1/userrights/changeprofile
    private static async Task<IResult> ChangeProfile([FromBody] ChangeProfileRequest? request, HttpRequest httpRequest,
        IMediator mediator, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Call {nameof(ChangeProfileCommandHandler)} from {nameof(ChangeProfile)}");
        if (request is null)
            return Results.BadRequest(CarcassApiErrors.RequestIsEmpty);
        var command = request.AdaptTo(httpRequest);
        var result = await mediator.Send(command, cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის პაროლის ცვლილება
    //შემავალი ინფორმაცია -> ChangePasswordModel კლასის ობიექტი
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული ინფორმაციის ვალიდურობა და ხდება პაროლის ცვლილებების დაფიქსირება
    // PUT api/v1/userrights/changepassword
    private static async Task<IResult> ChangePassword([FromBody] ChangePasswordRequest? request,
        HttpRequest httpRequest, IMediator mediator, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Call {nameof(ChangePasswordCommandHandler)} from {nameof(ChangePassword)}");
        if (request is null)
            return Results.BadRequest(CarcassApiErrors.RequestIsEmpty);
        var command = request.AdaptTo(httpRequest);
        var result = await mediator.Send(command, cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის წაშლა
    //შემავალი ინფორმაცია -> userName პარამეტრის სახით
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული userName პარამეტრის შიგთავსი ემთხვევა თუ არა მიმდინარე მომხმარებელს და
    //   თუ ემთხვევა, ხდება მიმდინარე მომხმარებლის წაშლა
    //მომავალში უნდა დაემატოს -> იმის შემოწმება, არის თუ არა ამ მომხმარებლის სახელით გაკეთებული რამე სამუშაო.
    //  თუ მომხმარებელი სადმე არის მითითებული, მაშინ მისი წაშლა არ უნდა მოხდეს.
    //  თუ მაინც გახდა საჭირო მომავალში მომხმარებლის წაშლა, უნდა აეწყოს მომხმარებლის ჩანაწერების გადაბარების მექანიზმი
    //  რის მერეც შესაძლებელი გახდება მომხმარებლის იდენტიფიკატორის გათავისუფლება კავშირებისაგან და წაშლაც მოხერხდება
    // DELETE api/v1/userrights/deletecurrentuser/{userName}
    private static async Task<IResult> DeleteCurrentUser(string userName, HttpRequest httpRequest, IMediator mediator,
        CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Call {nameof(DeleteCurrentUserCommandHandler)} from {nameof(DeleteCurrentUser)}");
        var command = new DeleteCurrentUserCommandRequest(httpRequest) { UserName = userName };
        var result = await mediator.Send(command, cancellationToken);
        return result.Match(_ => Results.Ok(), Results.BadRequest);
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის უფლებების შესაბამისი მენიუს შესახებ ინფორმაციის ჩატვირთვა
    //შემავალი ინფორმაცია -> არა
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> რეპოზიტორიას გადაეწოდება მიმდინარე მომხმარებლის სახელი და
    //  მისი უფლებების მიხედვით ჩატვირთული მენიუს შესახებ ინფორმაციას უბრუნებს გამომძახებელს
    // GET api/v1/userrights/getmainmenu
    private static async Task<IResult> MainMenu(HttpRequest httpRequest, IMediator mediator,
        CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Call {nameof(MainMenuQueryHandler)} from {nameof(MainMenu)}");
        var query = new MainMenuQueryRequest(httpRequest);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }
}