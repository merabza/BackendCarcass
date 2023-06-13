using Microsoft.AspNetCore.Mvc;

namespace ServerCarcass.Controllers;

//ამ კონტროლერის დანიშნულებაა შემოწმდეს ცოცხალია თუ არა პროგრამა
[ApiController]
[Route("api/testconnection")]
public sealed class TestConnectionController : Controller //რეფაქტორინგი გაკეთებულია
{
    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კავშირის შემოწმების საშუალება
    //შემავალი ინფორმაცია -> არა
    //უფლება -> შემოწმება საჭირო არ არის
    //მოქმედება -> უბრალოდ აბრუნებს 200 კოდს. თუ ამ მეთოდმა იმუშავა, კლიენტი მიხვდება, რომ პროგრამა გაშვებულია
    [HttpGet]
    public ActionResult<bool> Test()
    {
        return Ok(true);
    }
}