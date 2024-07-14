using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.Handlers.DataTypes;
using BackendCarcassApi.QueryRequests.DataTypes;
using BackendCarcassContracts.V1.Routes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WebInstallers;

namespace BackendCarcassApi.Endpoints.V1;

//შესასვლელი წერტილი -> გამოიყენება DataTypes ცხრილის ინფორმაციის ჩასატვირთად
//ცალკე ხდება ცხრილების მოდელების მიღება, რომელიც ასევე DataTypes ცხრილში ინახება
//[ApiController]
//[Route("api/[controller]")]
//[Authorize]

// ReSharper disable once UnusedType.Global
public sealed class DataTypesEndpoints : IInstaller
{
    public int InstallPriority => 70;

    public int ServiceUsePriority => 70;

    //private static int _lastSequentialNumber;
    public void InstallServices(WebApplicationBuilder builder, string[] args, Dictionary<string, string> parameters)
    {
    }

    public void UseServices(WebApplication app)
    {
        //Console.WriteLine("DataTypesEndpoints.UseServices Started");
        var group = app.MapGroup(CarcassApiRoutes.DataTypes.DataTypesBase).RequireAuthorization();

        group.MapGet(CarcassApiRoutes.DataTypes.DataTypesList, DataTypesList);
        group.MapGet(CarcassApiRoutes.DataTypes.GridModel, GridModel);
        group.MapGet(CarcassApiRoutes.DataTypes.MultipleGridModels, MultipleGridModels);
        //Console.WriteLine("DataTypesEndpoints.UseServices Finished");
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> DataType ცხრილში არსებული ყველა ჩანაწერის ჩატვირთვა და დაბრუნება გამომძახებელს
    //შემავალი ინფორმაცია -> არ არის
    //უფლება -> ჩაიტვირთება მხოლოდ იმ ცხრილების შესახებ ინფორმაცია, რომლებზეც უფლება აქვს მიმდინარე მომხმარებელს.
    //მოქმედება -> ხდება DataType ცხრილის ყველა ჩანაწერის ჩატვირთვა, ოღონდ ველი სადაც ინახება ცხრილების მოდელები
    //   არ ჩაიტვირთება. ასე კეთდება სისწრაფისათვის. ცხრილების მოდელების ჩატვირთვა ხდება ცალკე
    //[HttpGet("getdatatypes")]
    private static async Task<IResult> DataTypesList(HttpRequest request, IMediator mediator,
        CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Call {nameof(DataTypesListQueryHandler)} from {nameof(DataTypesList)}");
        var query = new DataTypesQueryRequest(request);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> DataType ცხრილში არსებული ცხრილის მოდელის ჩატვირთვა და დაბრუნება
    //შემავალი ინფორმაცია -> tableName იმ ცხრილის სახელი, რომელიც შესაბამისი ცხრილის მოდელიც უნდა ჩაიტვირთოს
    //უფლება -> tableName ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს tableName ცხრილის ნახვის უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება DataType ცხრილის შესაბამისი ჩანაწერის მოძებნა და იქიდან ჩაიტვირთება ცხრილის მოდელი
    //   ჩატვირთული ინფორმაცია უბრუნდება გამომძახებელს
    //[HttpGet("getgridmodel/{tableName}")]
    private static async Task<IResult> GridModel(string gridName, IMediator mediator,
        CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Call {nameof(GridModelQueryHandler)} from {nameof(GridModel)}");
        var query = new GridModelQueryRequest(gridName);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }


    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> DataType ცხრილში არსებული ცხრილის მოდელების ჩატვირთვა მოწოდებული ცხრილებისათვის და დაბრუნება
    //შემავალი ინფორმაცია -> grids ცხრილების ჩამონათვალი, რომლების მოდელებიც უნდა ჩაიტვირთოს
    //უფლება -> grids სიაში არსებული ყველა ცხრილის ნახვის უფლება
    //მოქმედება -> პირველ რიგში ხდება მოწოდებული მოთხოვნის სტრიქონის გაანალიზება
    //   სათითაოდ ყველა ცხრილის სახელისათვის მოწმდება აქვს თუ არა მომხმარებელს ამ ცხრილის ნახვის უფლება.
    //   თუ რომელიმე ცხრილის ნახვის უფლება არ აქვს მომხმარებელს, საერთოდ არცერთის შესახებ ინფორმაცია არ ბრუნდება
    //   მიუხედავად იმისა აქვს თუ არა დანარჩენზე უფლება
    //   თუ ყველა ცხრილზე აქვს უფლება, თითოეული ცხრილისათვის ხდება DataType ცხრილის შესაბამისი ჩანაწერის მოძებნა
    //   და იქიდან ჩაიტვირთება ცხრილის მოდელი
    //   ჩატვირთული მოდელების სია უბრუნდება გამომძახებელს
    //საჭიროა იმ შემთხვევებისათვის, როცა ერთდროულად რამდენიმე ცხრილი უნდა ჩაიტვირთოს.
    //ამ დროს საჭიროა იმის ცოდნა, რომელ სხვა ცხრილებს იყენებენ ჩასატვირთი ცხრილები
    //შესაბამისად ეს ინფორმაცია კი ინახება ცხრილების მოდელებში, რისი ჩატვირთვაც აქ ხდება.
    //query like this: example.com/api/forms/getmultiplegridrules?grids=gridName1&grids=gridName2&grids=gridName3
    //[HttpGet("getmultiplegridrules")]
    private static async Task<IResult> MultipleGridModels(HttpRequest request, IMediator mediator,
        CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Call {nameof(MultipleGridModelsQueryHandler)} from {nameof(MultipleGridModels)}");
        var query = new MultipleGridModelsQueryRequest(request);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match(Results.Ok, Results.BadRequest);
    }
}