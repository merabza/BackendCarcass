using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CarcassRepositories;
using CarcassRepositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ServerCarcass.Controllers;

//კონტროლერი -> გამოიყენება DataTypes ცხრილის ინფორმაციის ჩასატვირთად
//ცალკე ხდება ცხრილების მოდელების მიღება, რომელიც ასევე DataTypes ცხრილში ინახება
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DataTypesController : Controller //რეფაქტორინგი გაკეთებულია
{
    private readonly ILogger<DataTypesController> _logger;
    private readonly IDbRepository _repository;

    public DataTypesController(IDbRepository repo,
        //IMasterDataRepository mdRepo, 
        ILogger<DataTypesController> logger)
    {
        _repository = repo;
        //_mdRepo = mdRepo;
        _logger = logger;
    }

    //private readonly IMasterDataRepository _mdRepo;
    private string? CurrentUserName => HttpContext.User.Claims.SingleOrDefault(so => so.Type == ClaimTypes.Name)?.Value;

    //private bool HasUserRightRoleAccessTable(string tableName)
    //{
    //  return _mdRepo.CheckTableViewRight(HttpContext.User.Claims, tableName);
    //}

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> DataType ცხრილში არსებული ყველა ჩანაწერის ჩატვირთვა და დაბრუნება გამომძახებელს
    //შემავალი ინფორმაცია -> არ არის
    //უფლება -> ჩაიტვირთება მხოლოდ იმ ცხრილების შესახებ ინფორმაცია, რომლებზეც უფლება აქვს მიმდინარე მომხმარებელს.
    //მოქმედება -> ხდება DataType ცხრილის ყველა ჩანაწერის ჩატვირთვა, ოღონდ ველი სადაც ინახება ცხრილების მოდელები
    //   არ ჩაიტვირთება. ასე კეთდება სისწრაფისათვის. ცხრილების მოდელების ჩატვირთვა ხდება ცალკე
    [HttpGet("getdatatypes")]
    public ActionResult<DataTypeFfModel[]> GetDataTypes()
    {
        try
        {
            if (CurrentUserName is null)
                return BadRequest("მომხმარებელი არასწორია");
            var res = _repository.GetDataTypes(CurrentUserName);
            //if (res == null)
            //    return BadRequest("data types not loaded");
            return Ok(res);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("error when get data");
        }
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> DataType ცხრილში არსებული ცხრილის მოდელის ჩატვირთვა და დაბრუნება
    //შემავალი ინფორმაცია -> tableName იმ ცხრილის სახელი, რომლიც შესაბამისი ცხრილის მოდელიც უნდა ჩაიტვირთოს
    //უფლება -> tableName ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს tableName ცხრილის ნახვის უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება DataType ცხრილის შესაბამისი ჩანაწერის მოძებნა და იქიდან ჩაიტვირთება ცხრილის მოდელი
    //   ჩატვირთული ინფორმაცია უბრუნდება გამომძახებელს
    [HttpGet("getgridmodel/{tableName}")]
    public ActionResult<string> GetGridModel(string tableName)
    {
        //if (!HasUserRightRoleAccessTable(tableName)) 
        //  return BadRequest();

        try
        {
            var res = _repository.GridModel(tableName);
            if (res == null)
                return BadRequest($"Grid with key {tableName} not found");
            return Ok(res);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("error when get data");
        }
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
    [HttpGet("getmultiplegridrules")]
    public ActionResult<Dictionary<string, string>> GetMultipleGridModels()
    {
        Dictionary<string, string> resultList = new();
        try
        {
            //შემოწმდეს მოწოდებული მოთხოვნა
            var reqQuery = Request.Query["grids"];
            if (reqQuery.Count == 0)
                return BadRequest("no grid names in uri query");

            //დამზადდეს ჩასატვირთი მოდელების შესაბამისი ცხრილების სახელების სია.
            //სიის დამზადება საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მისი მეორედ გავლა
            //პირველი გავლისას მოწმდება უფლებები
            List<string> tableNames = reqQuery.Distinct().ToList();
            //if (tableNames.Any(tableName => !HasUserRightRoleAccessTable(tableName)))
            //  return BadRequest();
            //ხოლო მეორე გავლისას ხდება უშუალოდ საჭირო ინფორმაციის ჩატვირთვა
            foreach (var tableName in tableNames)
            {
                var res = _repository.GridModel(tableName);
                if (res == null)
                    return BadRequest($"Grid with key {tableName} not found");
                resultList.Add(tableName, res);
            }

            return Ok(resultList);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("error when get data");
        }
    }
}