using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Api.Mappers;
using BackendCarcass.Application.UserRights.ChangePassword;
using BackendCarcass.Application.UserRights.ChangeProfile;
using BackendCarcass.Application.UserRights.DeleteCurrentUser;
using BackendCarcass.Application.UserRights.GetMainMenu;
using BackendCarcass.Repositories.Models;
using BackendCarcassShared.Contracts.Errors;
using BackendCarcassShared.Contracts.V1.Requests;
using BackendCarcassShared.Contracts.V1.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using WebSystemTools.WebApi.Abstractions.Infrastructure;

namespace BackendCarcass.Api.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public static class UserRightsEndpoints
{
    public static bool UseUserRightsEndpoints(this IEndpointRouteBuilder endpoints, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseUserRightsEndpoints));

        RouteGroupBuilder group = endpoints
            .MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.UserRights.UserRightsBase).RequireAuthorization();

        group.MapGet(CarcassApiRoutes.UserRights.IsCurrentUserValid, IsCurrentUserValid);
        group.MapPut(CarcassApiRoutes.UserRights.ChangeProfile, ChangeProfile);
        group.MapPut(CarcassApiRoutes.UserRights.ChangePassword, ChangePassword);
        group.MapDelete(CarcassApiRoutes.UserRights.DeleteCurrentUser, DeleteCurrentUser);
        group.MapGet(CarcassApiRoutes.UserRights.MainMenu, MainMenu);

        debugLogger?.Information("{MethodName} Finished", nameof(UseUserRightsEndpoints));

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
    private static async ValueTask<Results<Ok, BadRequest<Error>, ProblemHttpResult>> ChangeProfile(
        [FromBody] ChangeProfileRequest? request, ICommandHandler<ChangeProfileRequestCommand> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ChangeProfileCommandHandler)} from {nameof(ChangeProfile)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ChangeProfileRequestCommand command = request.AdaptTo();
        Result result = await handler.Handle(command, cancellationToken);

        return result.Match<Results<Ok, BadRequest<Error>, ProblemHttpResult>>(() => TypedResults.Ok(),
            errors => (ProblemHttpResult)CustomResults.Problem(errors));

        //return result.Match<Results<Ok, BadRequest<ErrorOmd[]>>>(_ => TypedResults.Ok(),
        //    errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის პაროლის ცვლილება
    //შემავალი ინფორმაცია -> ChangePasswordModel კლასის ობიექტი
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული ინფორმაციის ვალიდურობა და ხდება პაროლის ცვლილებების დაფიქსირება
    // PUT api/v1/userrights/changepassword
    private static async ValueTask<Results<Ok, BadRequest<Error>, ProblemHttpResult>> ChangePassword(
        [FromBody] ChangePasswordRequest? request, ICommandHandler<ChangePasswordRequestCommand> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ChangePasswordCommandHandler)} from {nameof(ChangePassword)}");
        if (request is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        ChangePasswordRequestCommand command = request.AdaptTo();
        Result result = await handler.Handle(command, cancellationToken);
        return result.Match<Results<Ok, BadRequest<Error>, ProblemHttpResult>>(() => TypedResults.Ok(),
            errors => (ProblemHttpResult)CustomResults.Problem(errors));
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
    private static async ValueTask<Results<Ok, ProblemHttpResult>> DeleteCurrentUser(string userName,
        ICommandHandler<DeleteCurrentUserRequestCommand> handler, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(DeleteCurrentUserCommandHandler)} from {nameof(DeleteCurrentUser)}");
        var command = new DeleteCurrentUserRequestCommand { UserName = userName };
        Result result = await handler.Handle(command, cancellationToken);
        return result.Match<Results<Ok, ProblemHttpResult>>(() => TypedResults.Ok(),
            errors => (ProblemHttpResult)CustomResults.Problem(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის უფლებების შესაბამისი მენიუს შესახებ ინფორმაციის ჩატვირთვა
    //შემავალი ინფორმაცია -> არა
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> რეპოზიტორიას გადაეწოდება მიმდინარე მომხმარებლის სახელი და
    //  მისი უფლებების მიხედვით ჩატვირთული მენიუს შესახებ ინფორმაციას უბრუნებს გამომძახებელს
    // GET api/v1/userrights/getmainmenu
    private static async Task<Results<Ok<MainMenuModel>, ProblemHttpResult>> MainMenu(
        IQueryHandler<MainMenuRequestQuery, MainMenuModel> handler, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(MainMenuQueryHandler)} from {nameof(MainMenu)}");
        var query = new MainMenuRequestQuery();
        Result<MainMenuModel> result = await handler.Handle(query, cancellationToken);
        return result.Match<MainMenuModel, Results<Ok<MainMenuModel>, ProblemHttpResult>>(res => TypedResults.Ok(res),
            errors => (ProblemHttpResult)CustomResults.Problem(errors));
    }
}
