using CarcassRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarcassContracts.V1.Responses;

namespace ServerCarcass.Controllers;

//კონტროლერი -> გამოიყენება გაშვებული პროცესების მიმდინარეობის გასაკონტროლებლად.
//რადგან ჯერჯერობით გაშვებული პროცესები არ გვაქვს, ეს კონტროლერი ჯერ-ჯერობით საჭირო არ არის.
//თუმცა მომავალში, როცა სერვისებთან ურთიერთობა იქნება საჭიროა, შეიძლება ეს კონტროლერი აღვადგინო.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ProcessesController : Controller //რეფაქტორინგი დროებით შეჩერებულია
{
    private readonly IDbRepository _repository;

    public ProcessesController(IDbRepository repo)
    {
        _repository = repo;
    }


    [HttpGet("getstatus/{commandkey}")]
    public ActionResult<CommandRunningStatus> Getstatus(int userId, int viewStyle)
    {
        var
            commandRunningStatus = new CommandRunningStatus(); // _repository.GetTreeData(userId, viewStyle);
        //string res = JsonConvert.SerializeObject(mainMenuModel);
        //return Ok(res);
        return Ok(commandRunningStatus);
    }
}