using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassApi.Filters;
using BackendCarcassApi.Handlers.UserRights;
using BackendCarcassApi.Mappers;
using BackendCarcassContracts.Errors;
using BackendCarcassContracts.V1.Requests;
using BackendCarcassContracts.V1.Routes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebInstallers;

namespace BackendCarcassApi.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public sealed class UserRightsEndpoints : IInstaller
{
    public int InstallPriority => 70;
    public int ServiceUsePriority => 70;

    public bool InstallServices(WebApplicationBuilder builder, bool debugMode, string[] args,
        Dictionary<string, string> parameters)
    {
        return true;
    }

    public bool UseServices(WebApplication app, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Started");

        var group = app.MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.UserRights.UserRightsBase)
            //.RequireCors(CorsInstaller.MyAllowSpecificOrigins)
            .RequireAuthorization()
            .AddEndpointFilter<UserNameFilter>();

        group.MapGet(CarcassApiRoutes.UserRights.IsCurrentUserValid, IsCurrentUserValid);
        group.MapPut(CarcassApiRoutes.UserRights.ChangeProfile, ChangeProfile);
        group.MapPut(CarcassApiRoutes.UserRights.ChangePassword, ChangePassword);
        group.MapDelete(CarcassApiRoutes.UserRights.DeleteCurrentUser, DeleteCurrentUser);
        group.MapGet(CarcassApiRoutes.UserRights.MainMenu, MainMenu);

        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Finished");

        return true;
    }

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