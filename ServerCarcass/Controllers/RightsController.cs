using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarcassRepositories;
using CarcassRepositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServerCarcass.Controllers;

//კონტროლერი -> აქ რეალიზებულია უფლებების ფორმის მუშაობისათვის საჭირო ყველა ქმედება
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class RightsController : Controller //რეფაქტორინგი გაკეთებულია
{
    private const string UserNotIdentified = "მომხმარებელს არ აქვს საკმარისი უფლებები";
    private const string ErrorLoadingData = "შეცდომა მონაცემების ჩატვირთვისას";
    private readonly ILogger<RightsController> _logger;
    private readonly IMasterDataRepository _mdRepo;
    private readonly IDbRepository _repository;


    public RightsController(IDbRepository repo, IMasterDataRepository mdRepo, ILogger<RightsController> logger)
    {
        _repository = repo;
        _mdRepo = mdRepo;
        _logger = logger;
    }

    //private int CurrentUserId => int.TryParse(HttpContext.User.Identity.Name, out int userId) ? userId : 0;
    private string? CurrentUserName => HttpContext.User.Claims.SingleOrDefault(so => so.Type == ClaimTypes.Name)?.Value;


    private bool HasUserRightRole()
    {
        return _mdRepo.CheckMenuRight(HttpContext.User.Claims, "Rights");
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარცხენა ნაწილის (მშობლების) ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> viewStyle ხედის სტილი. სულ არის ოსი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    [HttpGet("getparentstreedata/{viewStyle}")]
    public ActionResult<RightsTreeDataModel> GetParentsTreeData(int viewStyle)
    {
        try
        {
            if (!HasUserRightRole())
                return BadRequest(UserNotIdentified);
            if (CurrentUserName is null)
                return BadRequest(UserNotIdentified);
            var rightsTreeData =
                _mdRepo.GetParentsTreeData(CurrentUserName, (ERightsEditorViewStyle)viewStyle);
            return Ok(rightsTreeData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetParentsTreeData)}.");
            return BadRequest(ErrorLoadingData);
        }
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარჯვენა ნაწილის (შვილების) ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> 1) dataTypeKey არჩეული მშობლის კოდი, 2) viewStyle ხედის სტილი. სულ არის ოსი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    [HttpGet("getchildrentreedata/{dataTypeKey}/{viewStyle}")]
    public ActionResult<RightsTreeDataModel> GetChildrenTreeData(string dataTypeKey, int viewStyle)
    {
        try
        {
            if (!HasUserRightRole())
                return BadRequest(UserNotIdentified);
            if (CurrentUserName is null)
                return BadRequest(UserNotIdentified);
            var rightsTreeData =
                _mdRepo.GetChildrenTreeData(CurrentUserName, dataTypeKey, (ERightsEditorViewStyle)viewStyle);
            return Ok(rightsTreeData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetChildrenTreeData)}.");
            return BadRequest(ErrorLoadingData);
        }
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის მარჯვენა ნაწილის (შვილების) მხარეს მონიშვნების შესახებ ინფორმაციის ჩატვირთვა ბაზიდან
    //შემავალი ინფორმაცია -> 1) dataTypeId არჩეული მშობლის ტიპი,
    //   2) dataTypeKey არჩეული მშობლის კოდი,
    //   3) viewStyle ხედის სტილი. სულ არის ორი სტილი: ჩვეულებრივი და რევერსული
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს ხდება მხოლოდ იმ ინფორმაციის ჩატვირთვა და დაბრუნება, რაზეც უფლება აქვს მიმდინარე მომხმარებელს
    //   თუ რა ინფორმაცია უნდა ჩაიტვირთოს ეს რეპოზიტორიის მხარეს განისაზღვრება მიწოდებული პარამეტრების საფუძველზე
    [HttpGet("halfchecks/{dataTypeId}/{dataKey}/{viewStyle}")]
    public ActionResult<RightsHalfChecksModel> GetHalfChecks(int dataTypeId, string dataKey, int viewStyle)
    {
        try
        {
            if (!HasUserRightRole())
                return BadRequest(UserNotIdentified);
            if (CurrentUserName is null)
                return BadRequest(UserNotIdentified);
            var rightsHalfChecks = _repository.HalfChecks(CurrentUserName, dataTypeId, dataKey,
                (ERightsEditorViewStyle)viewStyle);
            return Ok(rightsHalfChecks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetHalfChecks)}.");
            return BadRequest(ErrorLoadingData);
        }
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ფორმის საშუალებით განხორციელებული ცვლილებების შენახვა.
    //შემავალი ინფორმაცია -> 1) RightsChangeModel კლასის ობიექტების სია
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს, დგინდება, აქვს თუ არა მიმდინარე მომხმარებელს უფლება მოწოდებულ ინფორმაციაზე.
    //   თუ აღმოჩნდა, რომ რომელიმე ინფორმაციაზე უფლება არ აქვს, მისი შესაბამისი ცვლილების შენახვა არ ხდება.
    //   რაზეც უფლება აქვს ისინი ინახება.
    //   საბოლოოდ ამ უფლებების შემოწმება ხდება რეპოზიტორიის მხარეს.
    [HttpPost("savedata")]
    public async Task<ActionResult<bool>> SaveData()
    {
        try
        {
            if (!HasUserRightRole())
                return BadRequest(UserNotIdentified);
            if (CurrentUserName is null)
                return BadRequest(UserNotIdentified);
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var changedRights = JsonConvert.DeserializeObject<List<RightsChangeModel>>(body) ??
                                new List<RightsChangeModel>();
            //CurrentUserId;//უნდა იყოს გამოყენებული
            //!!!გასაკეთებელია ის, რომ შენახვისას უნდა შემოწმდეს, ჰქონდა თუ არა უფლება მიმდინარე მომხმარებელს
            //შესაბამისი ინფორმაცია შეენახა
            return Ok(await _repository.SaveRightsChanges(CurrentUserName, changedRights));
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("შეცდომა უფლებების შესახებ ინფორმაციის შენახვისას");
        }
    }


    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> უფლებების ინფორმაციაში ბაზაში არსებული აცდენებისა და შეცდომების გასწორება.
    //შემავალი ინფორმაცია -> არ არის
    //მოქმედება -> მოწმდება აქვს თუ არა მომხმარებელს უფლებების ფორმაზე უფლება. თუ არა ბრუნდება უარი.
    //   თუ აქვს, ეშვება ოპტიმიზაციის პროცესი რეპოზიტორიის მხარეს.
    //   აქ დამატებით მომხმარებლის მონაცემებზე უფლებების შემოწმება არ ხდება,
    //   რადგან შეცდომები, რასაც ეს პროცედურა ასწორებს, ნებისმიერ შემთხვევაში გასასწორებელია
    [HttpPost("optimize")]
    public async Task<ActionResult<bool>> Optimize()
    {
        if (!HasUserRightRole())
            return BadRequest(UserNotIdentified);
        //ყურადღება!!! ოპტიმიზაცია არასწორად მუშაობს.
        //იწვევს საჭირო უფლებების განადგურებას.
        //სანამ არ გამოსწორდება, შემდეგი კოდი დაკომენტარებული უნდა დარჩეს
        //try
        //{
        //    return Ok(await _mdRepo.OptimizeRights());
        //}
        //catch (Exception e)
        //{
        //    _logger.Log(LogLevel.Error, e.Message);
        //    return BadRequest("შეცდომა უფლებების ოპტიმიზაციის პროცესის მიმდინარეობისას");
        //}
        return Ok(true);
    }
}