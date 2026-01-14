using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.Filters;
using BackendCarcassContracts.Errors;
using BackendCarcassContracts.V1.Routes;
using Carcass.Application.Rights.GetChildrenTree;
using Carcass.Application.Rights.GetHalfChecks;
using Carcass.Application.Rights.GetParentsTree;
using Carcass.Application.Rights.SaveRightsChanges;
using CarcassRights;
using CarcassRights.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Endpoints.V1;

//კონტროლერი -> აქ რეალიზებულია უფლებების ფორმის მუშაობისათვის საჭირო ყველა ქმედება
//[Authorize]
//[ApiController]
//[Route("api/[controller]")]

// ReSharper disable once UnusedType.Global
public static class RightsEndpoints
{
    public static bool UseRightsEndpoints(this IEndpointRouteBuilder endpoints, bool debugMode)
    {
        //if (debugMode)
        //    Console.WriteLine($"{nameof(UseRightsEndpoints)} Started");

        var group = endpoints.MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.Rights.RightsBase)
            .RequireAuthorization().AddEndpointFilter<UserMustHaveRightsEditorRightsFilter>();

        group.MapGet(CarcassApiRoutes.Rights.ParentsTreeData, ParentsTreeData);
        group.MapGet(CarcassApiRoutes.Rights.ChildrenTreeData, ChildrenTreeData);
        group.MapGet(CarcassApiRoutes.Rights.HalfChecks, HalfChecks);
        group.MapPost(CarcassApiRoutes.Rights.SaveData, SaveData);
        group.MapPost(CarcassApiRoutes.Rights.Optimize, Optimize);

        //if (debugMode)
        //    Console.WriteLine($"{nameof(UseRightsEndpoints)} Finished");

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარცხენა ნაწილის (მშობლების) ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> viewStyle ხედის სტილი. სულ არის ოსი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    //[HttpGet("getparentstreedata/{viewStyle}")]
    private static async Task<Results<Ok<List<DataTypeModel>>, BadRequest<Err[]>>> ParentsTreeData(int viewStyle,
        IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ParentsTreeDataQueryHandler)} from {nameof(ParentsTreeData)}");
        var query = new ParentsTreeDataRequestQuery((ERightsEditorViewStyle)viewStyle);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match<Results<Ok<List<DataTypeModel>>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარჯვენა ნაწილის (შვილების) ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> 1) dataTypeKey არჩეული მშობლის კოდი, 2) viewStyle ხედის სტილი. სულ არის ოსი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    //[HttpGet("getchildrentreedata/{dataTypeKey}/{viewStyle}")]
    private static async Task<Results<Ok<List<DataTypeModel>>, BadRequest<Err[]>>> ChildrenTreeData(string dataTypeKey,
        int viewStyle, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ChildrenTreeDataCommandHandler)} from {nameof(ChildrenTreeData)}");
        var query = new ChildrenTreeDataRequestCommand(dataTypeKey, (ERightsEditorViewStyle)viewStyle);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match<Results<Ok<List<DataTypeModel>>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარჯვენა ნაწილის (შვილების) მხარეს მონიშვნების შესახებ ინფორმაციის ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> 1) dataTypeId არჩეული მშობლის ტიპი,
    //   2) dataTypeKey არჩეული მშობლის კოდი,
    //   3) viewStyle ხედის სტილი. სულ არის ორი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    //[HttpGet("halfchecks/{dataTypeId}/{dataKey}/{viewStyle}")]
    private static async Task<Results<Ok<List<TypeDataModel>>, BadRequest<Err[]>>> HalfChecks(int dataTypeId,
        string dataKey, int viewStyle, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(HalfChecksCommandHandler)} from {nameof(HalfChecks)}");
        var query = new HalfChecksRequestCommand(dataTypeId, dataKey, (ERightsEditorViewStyle)viewStyle);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match<Results<Ok<List<TypeDataModel>>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის საშუალებით განხორციელებული ცვლილებების შენახვა.
    //შემავალი ინფორმაცია -> 1) RightsChangeModel კლასის ობიექტების სია
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს, დგინდება, აქვს თუ არა მიმდინარე მომხმარებელს უფლება მოწოდებულ ინფორმაციაზე.
    //   თუ აღმოჩნდა, რომ რომელიმე ინფორმაციაზე უფლება არ აქვს, მისი შესაბამისი ცვლილების შენახვა არ ხდება.
    //   რაზეც უფლება აქვს ისინი ინახება.
    //   საბოლოოდ ამ უფლებების შემოწმება ხდება რეპოზიტორიის მხარეს.
    //[HttpPost("savedata")]
    private static async ValueTask<Results<Ok<bool>, BadRequest<Err[]>>> SaveData(
        [FromBody] List<RightsChangeModel>? changesForSave, IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(SaveDataCommandHandler)} from {nameof(SaveData)}");
        if (changesForSave is null)
        {
            return TypedResults.BadRequest(Err.Create(CarcassApiErrors.RequestIsEmpty));
        }

        var commandRequest = new SaveDataRequestCommand(changesForSave);
        var result = await mediator.Send(commandRequest, cancellationToken);
        return result.Match<Results<Ok<bool>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ინფორმაციაში ბაზაში არსებული აცდენებისა და შეცდომების გასწორება.
    //შემავალი ინფორმაცია -> არ არის
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს, ეშვება ოპტიმიზაციის პროცესი რეპოზიტორიის მხარეს.
    //   აქ დამატებით მომხმარებლის მონაცემებზე უფლებების შემოწმება არ ხდება,
    //   რადგან შეცდომები, რასაც ეს პროცედურა ასწორებს, ნებისმიერ შემთხვევაში გასასწორებელია
    //[HttpPost("optimize")]
    private static Ok<bool> Optimize()
    {
        //Debug.WriteLine($"Call {nameof(OptimizeCommandHandler)} from {nameof(Optimize)}");
        //if (!HasUserRightRole(mdRepo, request))
        //    return Results.BadRequest(UserNotIdentified);
        //ყურადღება!!! ოპტიმიზაცია არასწორად მუშაობს.
        //იწვევს საჭირო უფლებების განადგურებას.
        //სანამ არ გამოსწორდება, შემდეგი კოდი დაკომენტარებული უნდა დარჩეს

        //try
        //{
        //    return Results.Ok(await mdRepo.OptimizeRights());
        //}
        //catch (Exception e)
        //{
        //    logger.Log(LogLevel.Error, e.Message);
        //    return Results.BadRequest("შეცდომა უფლებების ოპტიმიზაციის პროცესის მიმდინარეობისას");
        //}
        return TypedResults.Ok(true);
    }
}
