using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Api.Mappers;
using BackendCarcass.Application.UserRights.ChangePassword;
using BackendCarcass.Application.UserRights.ChangeProfile;
using BackendCarcass.Application.UserRights.DeleteCurrentUser;
using BackendCarcass.Application.UserRights.GetMainMenu;
using BackendCarcass.Repositories.Models;
using BackendCarcassContracts.Errors;
using BackendCarcassContracts.V1.Requests;
using BackendCarcassContracts.V1.Routes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OneOf;
using Serilog;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Api.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public static class UserRightsEndpoints
{
    public static bool UseUserRightsEndpoints(this IEndpointRouteBuilder endpoints, ILogger logger, bool debugMode)
    {
        if (debugMode)
        {
            logger.Information("{MethodName} Started", nameof(UseUserRightsEndpoints));
        }

        RouteGroupBuilder group = endpoints
            .MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.UserRights.UserRightsBase).RequireAuthorization();

        group.MapGet(CarcassApiRoutes.UserRights.IsCurrentUserValid, IsCurrentUserValid);
        group.MapPut(CarcassApiRoutes.UserRights.ChangeProfile, ChangeProfile);
        group.MapPut(CarcassApiRoutes.UserRights.ChangePassword, ChangePassword);
        group.MapDelete(CarcassApiRoutes.UserRights.DeleteCurrentUser, DeleteCurrentUser);
        group.MapGet(CarcassApiRoutes.UserRights.MainMenu, MainMenu);

        if (debugMode)
        {
            logger.Information("{MethodName} Finished", nameof(UseUserRightsEndpoints));
        }

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის შემოწმება
    //შემავალი ინფორმაცია -> არა
    //უფლება -> ნებისმიერი
    //მოქმედება -> თუ ამ მეთოდამდე მოვიდა კოდი, ეს ნიშნავს, რომ მომხმარებელს ავტორიზაცია აქვს გავლილი
    //   ამიტომ მეთოდი ყოველთვის აბრუნებს Ok()-ს
    // GET api/v1/userrights/iscurrentuservalid
    private static Ok IsCurrentUserValid()
    {
        return TypedResults.Ok();
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის შესახებ ინფორმაციის ცვლილება
    //შემავალი ინფორმაცია -> ChangeProfileModel კლასის ობიექტი
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული ინფორმაციის ვალიდურობა და ხდება პროფაილში ცვლილებების დაფიქსირება
    // GET api/v1/userrights/changeprofile
    private static async ValueTask<Results<Ok, BadRequest<Err[]>>> ChangeProfile(
        [FromBody] ChangeProfileRequest? request, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ChangeProfileCommandHandler)} from {nameof(ChangeProfile)}");
        if (request is null)
        {
            return TypedResults.BadRequest(Err.Create(CarcassApiErrors.RequestIsEmpty));
        }

        ChangeProfileRequestCommand command = request.AdaptTo();
        OneOf<Unit, Err[]> result = await mediator.Send(command, cancellationToken);
        return result.Match<Results<Ok, BadRequest<Err[]>>>(_ => TypedResults.Ok(),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის პაროლის ცვლილება
    //შემავალი ინფორმაცია -> ChangePasswordModel კლასის ობიექტი
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული ინფორმაციის ვალიდურობა და ხდება პაროლის ცვლილებების დაფიქსირება
    // PUT api/v1/userrights/changepassword
    private static async ValueTask<Results<Ok, BadRequest<Err[]>>> ChangePassword(
        [FromBody] ChangePasswordRequest? request, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ChangePasswordCommandHandler)} from {nameof(ChangePassword)}");
        if (request is null)
        {
            return TypedResults.BadRequest(Err.Create(CarcassApiErrors.RequestIsEmpty));
        }

        ChangePasswordRequestCommand command = request.AdaptTo();
        OneOf<Unit, Err[]> result = await mediator.Send(command, cancellationToken);
        return result.Match<Results<Ok, BadRequest<Err[]>>>(_ => TypedResults.Ok(),
            errors => TypedResults.BadRequest(errors));
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
    private static async ValueTask<Results<Ok, BadRequest<Err[]>>> DeleteCurrentUser(string userName,
        IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(DeleteCurrentUserCommandHandler)} from {nameof(DeleteCurrentUser)}");
        var command = new DeleteCurrentUserRequestCommand { UserName = userName };
        OneOf<Unit, Err[]> result = await mediator.Send(command, cancellationToken);
        return result.Match<Results<Ok, BadRequest<Err[]>>>(_ => TypedResults.Ok(),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის უფლებების შესაბამისი მენიუს შესახებ ინფორმაციის ჩატვირთვა
    //შემავალი ინფორმაცია -> არა
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> რეპოზიტორიას გადაეწოდება მიმდინარე მომხმარებლის სახელი და
    //  მისი უფლებების მიხედვით ჩატვირთული მენიუს შესახებ ინფორმაციას უბრუნებს გამომძახებელს
    // GET api/v1/userrights/getmainmenu
    private static async Task<Results<Ok<MainMenuModel>, BadRequest<Err[]>>> MainMenu(IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(MainMenuQueryHandler)} from {nameof(MainMenu)}");
        var query = new MainMenuRequestQuery();
        OneOf<MainMenuModel, Err[]> result = await mediator.Send(query, cancellationToken);
        return result.Match<Results<Ok<MainMenuModel>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }
}
