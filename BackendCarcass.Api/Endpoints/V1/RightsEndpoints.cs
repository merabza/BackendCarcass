using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Api.Filters;
using BackendCarcass.Application.Rights.GetChildrenTree;
using BackendCarcass.Application.Rights.GetHalfChecks;
using BackendCarcass.Application.Rights.GetParentsTree;
using BackendCarcass.Application.Rights.SaveRightsChanges;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using BackendCarcassShared.Contracts.Errors;
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

//კონტროლერი -> აქ რეალიზებულია უფლებების ფორმის მუშაობისათვის საჭირო ყველა ქმედება
//[Authorize]
//[ApiController]
//[Route("api/[controller]")]

// ReSharper disable once UnusedType.Global
public static class RightsEndpoints
{
    public static bool UseRightsEndpoints(this IEndpointRouteBuilder endpoints, ILogger? debugLogger)
    {
        debugLogger?.Information("{MethodName} Started", nameof(UseRightsEndpoints));

        RouteGroupBuilder group = endpoints.MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.Rights.RightsBase)
            .RequireAuthorization().AddEndpointFilter<UserMustHaveRightsEditorRightsFilter>();

        group.MapGet(CarcassApiRoutes.Rights.ParentsTreeData, ParentsTreeData);
        group.MapGet(CarcassApiRoutes.Rights.ChildrenTreeData, ChildrenTreeData);
        group.MapGet(CarcassApiRoutes.Rights.HalfChecks, HalfChecks);
        group.MapPost(CarcassApiRoutes.Rights.SaveData, SaveData);
        group.MapPost(CarcassApiRoutes.Rights.Optimize, Optimize);

        debugLogger?.Information("{MethodName} Finished", nameof(UseRightsEndpoints));

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარცხენა ნაწილის (მშობლების) ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> viewStyle ხედის სტილი. სულ არის ოსი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    //[HttpGet("getparentstreedata/{viewStyle}")]
    private static async Task<Results<Ok<List<DataTypeModel>>, ProblemHttpResult>> ParentsTreeData(int viewStyle,
        IQueryHandler<ParentsTreeDataRequestQuery, List<DataTypeModel>> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ParentsTreeDataQueryHandler)} from {nameof(ParentsTreeData)}");
        var query = new ParentsTreeDataRequestQuery((ERightsEditorViewStyle)viewStyle);
        Result<List<DataTypeModel>> result = await handler.Handle(query, cancellationToken);
        return result.Match<List<DataTypeModel>, Results<Ok<List<DataTypeModel>>, ProblemHttpResult>>(
            res => TypedResults.Ok(res), errors => (ProblemHttpResult)CustomResults.Problem(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარჯვენა ნაწილის (შვილების) ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> 1) dataTypeKey არჩეული მშობლის კოდი, 2) viewStyle ხედის სტილი. სულ არის ოსი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    //[HttpGet("getchildrentreedata/{dataTypeKey}/{viewStyle}")]
    private static async Task<Results<Ok<List<DataTypeModel>>, ProblemHttpResult>> ChildrenTreeData(string dataTypeKey,
        int viewStyle, ICommandHandler<ChildrenTreeDataRequestCommand, List<DataTypeModel>> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(ChildrenTreeDataCommandHandler)} from {nameof(ChildrenTreeData)}");
        var query = new ChildrenTreeDataRequestCommand(dataTypeKey, (ERightsEditorViewStyle)viewStyle);
        Result<List<DataTypeModel>> result = await handler.Handle(query, cancellationToken);
        return result.Match<List<DataTypeModel>, Results<Ok<List<DataTypeModel>>, ProblemHttpResult>>(
            res => TypedResults.Ok(res), errors => (ProblemHttpResult)CustomResults.Problem(errors));
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
    private static async Task<Results<Ok<List<TypeDataModel>>, ProblemHttpResult>> HalfChecks(int dataTypeId,
        string dataKey, int viewStyle, ICommandHandler<HalfChecksRequestCommand, List<TypeDataModel>> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(HalfChecksCommandHandler)} from {nameof(HalfChecks)}");
        var query = new HalfChecksRequestCommand(dataTypeId, dataKey, (ERightsEditorViewStyle)viewStyle);
        Result<List<TypeDataModel>> result = await handler.Handle(query, cancellationToken);
        return result.Match<List<TypeDataModel>, Results<Ok<List<TypeDataModel>>, ProblemHttpResult>>(
            res => TypedResults.Ok(res), errors => (ProblemHttpResult)CustomResults.Problem(errors));
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
    private static async ValueTask<Results<Ok<bool>, BadRequest<Error>, ProblemHttpResult>> SaveData(
        [FromBody] List<RightsChangeModel>? changesForSave, ICommandHandler<SaveDataRequestCommand, bool> handler,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(SaveDataCommandHandler)} from {nameof(SaveData)}");
        if (changesForSave is null)
        {
            return TypedResults.BadRequest(CarcassApiErrors.RequestIsEmpty);
        }

        var commandRequest = new SaveDataRequestCommand(changesForSave);
        Result<bool> result = await handler.Handle(commandRequest, cancellationToken);
        return result.Match<bool, Results<Ok<bool>, BadRequest<Error>, ProblemHttpResult>>(res => TypedResults.Ok(res),
            errors => (ProblemHttpResult)CustomResults.Problem(errors));
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
        //    logger.Log(LogLevel.ErrorOmd, e.Message);
        //    return Results.BadRequest("შეცდომა უფლებების ოპტიმიზაციის პროცესის მიმდინარეობისას");
        //}
        return TypedResults.Ok(true);
    }
}
