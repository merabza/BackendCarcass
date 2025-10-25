﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.MasterData;
using BackendCarcassApi.Filters;
using BackendCarcassApi.Handlers.MasterData;
using BackendCarcassApi.QueryRequests.MasterData;
using BackendCarcassApi.QueryResponses;
using BackendCarcassContracts.V1.Routes;
using CarcassMasterDataDom.Models;
using LibCrud.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SystemToolsShared.Errors;
using WebInstallers;

namespace BackendCarcassApi.Endpoints.V1;

//უნივერსალური მექანიზმი ნებისმიერი ცხრილის ჩასატვირთად და დასარედაქტირებლად ბაზაში.

// ReSharper disable once UnusedType.Global
public sealed class MasterDataEndpoints : IInstaller
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

        var group = app.MapGroup(CarcassApiRoutes.ApiBase + CarcassApiRoutes.MasterData.MasterDataBase)
            .RequireAuthorization();

        //group.MapGet(CarcassApiRoutes.MasterData.All, AllRecords).AddEndpointFilter<UserTableRightsFilter>();
        group.MapGet(CarcassApiRoutes.MasterData.GetTables, GetTables);
        group.MapGet(CarcassApiRoutes.MasterData.GetLookupTables, GetLookupTables);
        group.MapGet(CarcassApiRoutes.MasterData.GetTableRowsData, GetTableRowsData)
            .AddEndpointFilter<UserTableRightsFilter>();
        group.MapGet(CarcassApiRoutes.MasterData.Get, MdGetOneRecord).AddEndpointFilter<UserTableRightsFilter>();
        group.MapPost(CarcassApiRoutes.MasterData.Post, MdCreateOneRecord).AddEndpointFilter<UserTableRightsFilter>();
        group.MapPut(CarcassApiRoutes.MasterData.Put, MdUpdateOneRecord).AddEndpointFilter<UserTableRightsFilter>();
        group.MapDelete(CarcassApiRoutes.MasterData.Delete, MdDeleteOneRecord)
            .AddEndpointFilter<UserTableRightsFilter>();

        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Finished");

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> tableName ცხრილში არსებული ყველა ჩანაწერის ჩატვირთვა და დაბრუნება გამომძახებელს
    //შემავალი ინფორმაცია -> tableName - ჩასატვირთი ცხრილის სახელი
    //უფლება -> tableName ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება tableName ცხრილის ნახვის უფლება.
    //  თუ ეს უფლება არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ ეს უფლება აქვს მიმდინარე მომხმარებელს, მოხდება tableName ცხრილის ჩატვირთვა და გამომძახებლისთვის დაბრუნება
    // GET api/v1/masterdata/{tableName}
    //private static async Task<IResult> AllRecords(HttpRequest request, string tableName, IMediator mediator,
    //    CancellationToken cancellationToken = default)
    //{
    //    Debug.WriteLine($"Call {nameof(AllRecordsQueryHandler)} from {nameof(AllRecords)}");
    //    var query = new MdGetTableAllRecordsQueryRequest(tableName, request);
    //    var result = await mediator.Send(query, cancellationToken);
    //    return result.Match(Results.Ok, Results.BadRequest);
    //}

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მოწოდებული ცხრილების სახელების მიხედვით ამ ცხრილების შიგთავსების ჩატვირთვა და დაბრუნება გამომძახებელს
    //შემავალი ინფორმაცია -> მოთხოვნა რომელიც შეიცავს ჩასატვირთი ცხრილების სიას
    //უფლება -> მოთხოვნაში არსებული ყველა ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება მოთხოვნაში არსებული ყველა ცხრილის ნახვის უფლება.
    //  თუ რომელიმე ცხრილის ნახვის უფლება არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ ეს ყველა ცხრილზე ნახვის უფლება აქვს მიმდინარე მომხმარებელს, მოხდება ყველა ცხრილის ჩატვირთვა და გამომძახებლისთვის დაბრუნება
    //query like this: localhost:3000/api/masterdata/gettables?tables=tableName1&tables=tableName2&tables=tableName3
    //deprecated
    public static async Task<Results<Ok<MdGetTablesQueryResponse>, BadRequest<Err[]>>> GetTables(StringValues tables,
        IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(GetTablesQueryHandler)} from {nameof(GetTables)}");
        var query = new MdGetTablesQueryRequest(tables);
        var result = await mediator.Send(query, cancellationToken);

        // Explicitly specify the type arguments for the Match method to resolve CS0411
        return result.Match<Results<Ok<MdGetTablesQueryResponse>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    // GET api/v1/masterdata/getlookuptables?tables=tableName1&tables=tableName2&tables=tableName3
    //public static async Task<Results<Ok<MdGetLookupTablesQueryResponse>, BadRequest<Err[]>>> GetLookupTables(
    //    HttpRequest request, IMediator mediator, CancellationToken cancellationToken = default)
    //{
    //    Debug.WriteLine($"Call {nameof(GetLookupTablesQueryHandler)} from {nameof(GetTables)}");
    //    var query = new MdGetLookupTablesQueryRequest(request);
    //    var result = await mediator.Send(query, cancellationToken);
    //    return result.Match<Results<Ok<MdGetLookupTablesQueryResponse>, BadRequest<Err[]>>>(
    //        res => TypedResults.Ok(res), errors => TypedResults.BadRequest(errors));
    //}

    public static async Task<Results<Ok<MdGetLookupTablesQueryResponse>, BadRequest<Err[]>>> GetLookupTables(
        StringValues tables, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(GetLookupTablesQueryHandler)} from {nameof(GetTables)}");
        var query = new MdGetLookupTablesQueryRequest(tables);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match<Results<Ok<MdGetLookupTablesQueryResponse>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    // GET api/v1/masterdata/gettablerowsdata/{tableName}
    public static async Task<Results<Ok<TableRowsData>, BadRequest<Err[]>>> GetTableRowsData(IMediator mediator,
        [FromRoute] string tableName, [FromQuery] string filterSortRequest,
        CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(GetTableRowsDataHandler)} from {nameof(GetTableRowsData)}");
        var queryNotes = new GetTableRowsDataQueryRequest(tableName, filterSortRequest);
        var resultNotes = await mediator.Send(queryNotes, cancellationToken);
        return resultNotes.Match<Results<Ok<TableRowsData>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

//შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კონკრეტული ცხრილის კონკრეტული ჩანაწერის ჩატვირთვა და გამომძახებლისთვის დაბრუნება
    //შემავალი ინფორმაცია -> 1) tableName ცხრილის სახელი, საიდანაც უნდა ჩაიტვირთოს ერთი ჩანაწერი
    //   2) id ჩანაწერის უნიკალური იდენტიფიკატორი.
    //უფლება -> tableName ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება tableName ცხრილის ნახვის უფლება.
    //  თუ tableName ცხრილის ნახვის უფლება არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ tableName ცხრილის ნახვის უფლება აქვს მიმდინარე მომხმარებელს,
    //   მოხდება id იდენტიფიკატორით ჩანაწერის ამოღება ბაზიდან და გამომძახებლისთვის დაბრუნება
    // GET api/v1/masterdata/{tableName}/{id}
    public static async Task<Results<Ok<MasterDataCrudLoadedData>, BadRequest<Err[]>>> MdGetOneRecord(string tableName,
        int id, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(MdGetOneRecordQueryHandler)} from {nameof(MdGetOneRecord)}");
        var query = new MdGetOneRecordQueryRequest(tableName, id);
        var result = await mediator.Send(query, cancellationToken);
        return result.Match<Results<Ok<MasterDataCrudLoadedData>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კონკრეტული ცხრილში ახალი ჩანაწერის ჩამატება
    //შემავალი ინფორმაცია -> 1) tableName ცხრილის სახელი
    //   2) მოთხოვნის ტანში ახალი ჩანაწერის შესაქმნელად საჭირო ინფორმაცია.
    //უფლება -> tableName ცხრილში ახალი ჩანაწერის დამატების უფლება
    //მოქმედება -> მოწმდება tableName ცხრილში ჩანაწერის დამატების უფლება.
    //  თუ ეს არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ აქვს, მოხდება მოთხოვნის ტანის გაანალიზება და მიღებული ახალი ჩანაწერის ბაზაში დამატება
    // POST api/v1/masterdata/{tableName}
    public static async Task<Results<Ok<MasterDataCrudLoadedData>, BadRequest<Err[]>>> MdCreateOneRecord(
        string tableName, HttpRequest request, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(MdCreateOneRecordCommandHandler)} from {nameof(MdCreateOneRecord)}");
        var commandRequest = new MdCreateOneRecordCommandRequest(tableName, request);
        var result = await mediator.Send(commandRequest, cancellationToken);
        return result.Match<Results<Ok<MasterDataCrudLoadedData>, BadRequest<Err[]>>>(res => TypedResults.Ok(res),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კონკრეტული ცხრილში კონკრეტული ჩანაწერის რედაქტირება
    //შემავალი ინფორმაცია -> 1) tableName ცხრილის სახელი
    //   2) id ჩანაწერის უნიკალური იდენტიფიკატორი
    //   3) მოთხოვნის ტანში შეცვლილი ჩანაწერის შესაბამისი ინფორმაცია.
    //უფლება -> tableName ცხრილში ჩანაწერის რედაქტირების უფლება
    //მოქმედება -> მოწმდება tableName ცხრილში ჩანაწერის რედაქტირების უფლება.
    //  თუ ეს არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ აქვს, მოხდება მოთხოვნის ტანის გაანალიზება და მიღებული შეცვლილი ჩანაწერის ბაზაში დაფიქსირება
    // PUT api/<controller>/<tableName>/5
    //[HttpPut("{tableName}/{id}")]
    public static async Task<Results<NoContent, BadRequest<Err[]>>> MdUpdateOneRecord(string tableName, int id,
        HttpRequest request, IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(MdUpdateOneRecordCommandHandler)} from {nameof(MdUpdateOneRecord)}");
        var commandRequest = new MdUpdateOneRecordCommandRequest(tableName, request, id);
        var result = await mediator.Send(commandRequest, cancellationToken);
        return result.Match<Results<NoContent, BadRequest<Err[]>>>(_ => TypedResults.NoContent(),
            errors => TypedResults.BadRequest(errors));
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კონკრეტული ცხრილში კონკრეტული ჩანაწერის წაშლა
    //შემავალი ინფორმაცია -> 1) tableName ცხრილის სახელი
    //   2) id ჩანაწერის უნიკალური იდენტიფიკატორი
    //უფლება -> tableName ცხრილში ჩანაწერის წაშლის უფლება
    //მოქმედება -> მოწმდება tableName ცხრილში ჩანაწერის წაშლის უფლება.
    //  თუ ეს არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ აქვს, მოხდება id იდენტიფიკატორის მიხედვით tableName ცხრილიდან ჩანაწერის წაშლა
    // DELETE api/<controller>/<tableName>/5
    //[HttpDelete("{tableName}/{id}")]
    public static async Task<Results<NoContent, BadRequest<Err[]>>> MdDeleteOneRecord(string tableName, int id,
        IMediator mediator, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Call {nameof(MdDeleteOneRecordCommandHandler)} from {nameof(MdDeleteOneRecord)}");
        var commandRequest = new MdDeleteOneRecordCommandRequest(tableName, id);
        var result = await mediator.Send(commandRequest, cancellationToken);
        return result.Match<Results<NoContent, BadRequest<Err[]>>>(_ => TypedResults.NoContent(),
            errors => TypedResults.BadRequest(errors));
    }
}