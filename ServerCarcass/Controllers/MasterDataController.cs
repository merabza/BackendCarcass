using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CarcassRepositories;
using CarcassShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ServerCarcass.Controllers;

//კონტროლერი -> უნივერსალური მექანიზმი ნებისმიერი ცხრილის ჩასატვირთად და დასარედაქტირებლად ბაზაში.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class MasterDataController : Controller //რეფაქტორინგი გაკეთებულია
{
    private readonly ILogger<MasterDataController> _logger;
    private readonly IMasterDataRepository _repository;

    public MasterDataController(IMasterDataRepository repo, ILogger<MasterDataController> logger)
    {
        _repository = repo;
        _logger = logger;
    }

    //private bool HasUserRightRoleAccessTable(string tableName)
    //{
    //  return _repository.CheckTableViewRight(HttpContext.User.Claims, tableName);
    //}

    private bool HasUserRightRoleAccessTableCrud(string tableName, ECrudOperationType crudType)
    {
        return _repository.CheckTableCrudRight(HttpContext.User.Claims, tableName, crudType);
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> tableName ცხრილში არსებული ყველა ჩანაწერის ჩატვირთვა და დაბრუნება გამომძახებელს
    //შემავალი ინფორმაცია -> tableName - ჩასატვირთი ცხრილის სახელი
    //უფლება -> tableName ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება tableName ცხრილის ნახვის უფლება.
    //  თუ ეს უფლება არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ ეს უფლება აქვს მიმდინარე მომხმარებელს, მოხდება tableName ცხრილის ჩატვირთვა და გამომძახებლისთვის დაბრუნება
    // GET: api/<controller>/<tableName>
    [HttpGet("{tableName}")]
    public ActionResult<IEnumerable<dynamic>> GetAll(string tableName)
    {
        try
        {
            ////შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს tableName ცხრილის ნახვის უფლება
            //if ( !HasUserRightRoleAccessTable(tableName))
            //  //თუ არა დაბრუნდეს შეცდომა
            //  return BadRequest();
            //ჩაიტვირთოს tableName ცხრილის ყველა ჩანაწერი.
            var res = _repository.GetEntitiesByTableName(tableName);
            if (res == null)
                return BadRequest($"table with name {tableName} not found");
            return Ok(res.Select(s => s.GetEditFields()));
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("error when get data");
        }
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მოწოდებული ცხრილების სახელების მიხედვით ამ ცხრილების შიგთავსების ჩატვირთვა და დაბრუნება გამომძახებელს
    //შემავალი ინფორმაცია -> მოთხოვნა რომელიც შეიცავს ჩასატვირთი ცხრილების სიას
    //უფლება -> მოთხოვნაში არსებული ყველა ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება მოთხოვნაში არსებული ყველა ცხრილის ნახვის უფლება.
    //  თუ რომელიმე ცხრილის ნახვის უფლება არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ ეს ყველა ცხრილზე ნახვის უფლება აქვს მიმდინარე მომხმარებელს, მოხდება ყველა ცხრილის ჩატვირთვა და გამომძახებლისთვის დაბრუნება
    //query like this: localhost:3000/api/masterdata/gettables?tables=tableName1&tables=tableName2&tables=tableName3
    [HttpGet("gettables")]
    public ActionResult<Dictionary<string, IEnumerable<dynamic>>> GetTables()
    {
        Dictionary<string, IEnumerable<dynamic>> resultList = new();
        try
        {
            var reqQuery = Request.Query["tables"];
            List<string> tableNames = reqQuery.Distinct().ToList();

            ////შემოწმდეს, ყველა მოწოდებულ ცხრილზე ნახვის უფლება აქვს თუ არა მომხმარებელს
            //if (tableNames.Any(tableName => !HasUserRightRoleAccessTable(tableName)))
            //  //თუ რომელიმეზე არ აქვს, დაბრუნდეს შეცდომა
            //  return BadRequest("თქვენ არ გაქვთ საკმარისი უფლებები");

            //ჩაიტვირთოს ყველა ცხრილი სათითაოდ
            foreach (var tableName in tableNames)
            {
                var tableResult = _repository.GetEntitiesByTableName(tableName);
                if (tableResult == null)
                    return BadRequest($"table with name {tableName} not found");
                var res = tableResult.Select(s => s.GetEditFields());
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


    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კონკრეტული ცხრილის კონკრეტული ჩანაწერის ჩატვირთვა და გამომძახებლისთვის დაბრუნება
    //შემავალი ინფორმაცია -> 1) tableName ცხრილის სახელი, საიდანაც უნდა ჩაიტვირთოს ერთი ჩანაწერი
    //   2) id ჩანაწერის უნიკალური იდენტიფიკატორი.
    //უფლება -> tableName ცხრილის ნახვის უფლება
    //მოქმედება -> მოწმდება tableName ცხრილის ნახვის უფლება.
    //  თუ tableName ცხრილის ნახვის უფლება არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ tableName ცხრილის ნახვის უფლება აქვს მიმდინარე მომხმარებელს,
    //   მოხდება id იდენტიფიკატორით ჩანაწერის ამოღება ბაზიდან და გამომძახებლისთვის დაბრუნება
    // GET api/<controller>/<tableName>/5
    [HttpGet("{tableName}/{id}")]
    public ActionResult<dynamic> Get(string tableName, int id)
    {
        try
        {
            ////შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს tableName ცხრილის ნახვის უფლება
            //if ( !HasUserRightRoleAccessTable(tableName))
            //  //თუ არა დაბრუნდეს შეცდომა
            //  return BadRequest();

            //tableName ცხრილის id ჩანაწერის ამოღება ბაზიდან
            var res = _repository.GetEntitiesByTableName(tableName);
            var idt = res?.SingleOrDefault(w => w.Id == id);
            if (idt == null)
                return NotFound($"in table {tableName} entry with identifier {id} not found");
            return Ok(idt.GetEditFields());
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("error when get entry");
        }
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კონკრეტული ცხრილში ახალი ჩანაწერის ჩამატება
    //შემავალი ინფორმაცია -> 1) tableName ცხრილის სახელი
    //   2) მოთხოვნის ტანში ახალი ჩანაწერის შესაქმნელად საჭირო ინფორმაცია.
    //უფლება -> tableName ცხრილში ახალი ჩანაწერის დამატების უფლება
    //მოქმედება -> მოწმდება tableName ცხრილში ჩანაწერის დამატების უფლება.
    //  თუ ეს არ აქვს მიმდინარე მომხმარებელს, ბრუნდება შეცდომა
    //  თუ აქვს, მოხდება მოთხოვნის ტანის გაანალიზება და მიღებული ახალი ჩანაწერის ბაზაში დამატება
    // POST api/<controller>/<tableName>
    [HttpPost("{tableName}")]
    //[FromBody] 
    public ActionResult<dynamic> Post(string tableName)
    {
        try
        {
            //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს tableName ცხრილში ჩანაწერის ჩამატების უფლება
            if (!HasUserRightRoleAccessTableCrud(tableName, ECrudOperationType.Create))
                //თუ არა დაბრუნდეს შეცდომა
                return BadRequest();
            //ამოვიღოთ მოთხოვნის ტანი
            using StreamReader reader = new(Request.Body);
            var body = reader.ReadToEnd();
            //გადავცეთ ტანი რეპოზიტორიას და მან იცის ამ ტანის გაანალიზება
            var newItem = _repository.AddEntityByTableName(tableName, body);
            if (newItem == null)
                return BadRequest($"Cannot insert new object in to table {tableName}");

            return (ActionResult<dynamic>)Ok(newItem.GetEditFields());
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("error when insert data");
        }
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
    [HttpPut("{tableName}/{id}")]
    public ActionResult Put(string tableName, int id)
    {
        try
        {
            //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს tableName ცხრილში ჩანაწერის შეცვლის უფლება
            if (!HasUserRightRoleAccessTableCrud(tableName, ECrudOperationType.Update))
                //თუ არა დაბრუნდეს შეცდომა
                return BadRequest();
            //ამოვიღოთ მოთხოვნის ტანი
            using StreamReader reader = new(Request.Body);
            var body = reader.ReadToEnd();
            //გადავცეთ ტანი რეპოზიტორიას და მან იცის ამ ტანის გაანალიზება
            if (_repository.UpdateEntityByTableName(tableName, id, body))
                return NoContent();
            return BadRequest($"Cannot update entry from table {tableName} with id {id}");
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("ცვლილებების შენახვა ბაზაში ვერ მოხერხდა");
        }
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
    [HttpDelete("{tableName}/{id}")]
    public ActionResult Delete(string tableName, int id)
    {
        try
        {
            //შემოწმდეს აქვს თუ არა მიმდინარე მომხმარებელს tableName ცხრილში ჩანაწერის წა უფლება
            if (!HasUserRightRoleAccessTableCrud(tableName, ECrudOperationType.Delete))
                //თუ არა დაბრუნდეს შეცდომა
                return BadRequest();
            //ამოვიღოთ მოთხოვნის ტანი
            var res = _repository.DeleteEntityByTableNameAndKey(tableName, id);
            if (!res)
                return BadRequest($"cannot delete entry from table {tableName} with id {id}");
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("წაშლის დროს მოხდა შეცდომა");
        }
    }
}