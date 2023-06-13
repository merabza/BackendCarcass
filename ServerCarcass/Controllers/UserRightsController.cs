using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarcassIdentity;
using CarcassJsonModels;
using CarcassRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ServerCarcass.Controllers;

//კონტროლერი -> რომელიც დანიშნულებაცაა მომხმარებლის პირადი ინფორმაციის მომსახურება
//აქ არსებული მოქმედებები დამატებით უფლებებს არ ითხოვს
//მთავარია ავტორიზაცია ჰქონდეს გავლილი მომხმარებელს
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UserRightsController : Controller //რეფაქტორინგი გაკეთებულია
{
    private readonly ILogger<UserRightsController> _logger;

    private readonly IMasterDataRepository _mdRepo;
    private readonly UserManager<AppUser> _userManager;

    public UserRightsController(IMasterDataRepository mdRepo, UserManager<AppUser> userManager,
        ILogger<UserRightsController> logger)
    {
        _mdRepo = mdRepo;
        _userManager = userManager;
        _logger = logger;
    }

    //private int CurrentUserId => int.TryParse(HttpContext.User.Identity.Name, out int userId) ? userId : 0;
    private string? CurrentUserName => HttpContext.User.Claims.SingleOrDefault(so => so.Type == ClaimTypes.Name)?.Value;

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის შემოწმება
    //შემავალი ინფორმაცია -> არა
    //უფლება -> ნებისმიერი
    //მოქმედება -> თუ ამ მეთოდამდე მოვიდა კოდი, ეს ნიშნავს, რომ მომხმარებელს ავტორიზაცია აქვს გავლილი
    //   ამიტომ მეთოდი ყოველთვის აბრუნებს Ok()-ს
    // GET: api/<controller>/iscurrentuservalid
    [HttpGet("iscurrentuservalid")]
    public ActionResult IsCurrentUserValid()
    {
        return Ok();
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის შესახებ ინფორმაციის ცვლილება
    //შემავალი ინფორმაცია -> ChangeProfileModel კლასის ობიექტი
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული ინფორმაციის ვალიდურობა და ხდება პროფაილში ცვლილებების დაფიქსირება
    // GET: api/<controller>/changeprofile
    [HttpPut("changeprofile")]
    public ActionResult ChangeProfile()
    {
        try
        {
            ////მივიღოთ ინფორმაცია მიღებული შეტყობინების ტანიდან
            //string body;
            //using (var reader = new StreamReader(Request.Body))
            //  body = reader.ReadToEnd();
            ////Json გადავიყვანოთ მოსალოდნელ ობიექტში
            //ChangeProfileModel chpData = JsonConvert.DeserializeObject<ChangeProfileModel>(body);

            ////შევამოწმოთ მიღებული ინფორმაცია ვალიდურია თუ არა
            //if (!TryValidateModel(chpData, nameof(ChangeProfileModel)))
            //{
            //  string errMessage =
            //    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(s => s.ErrorMessage));
            //  return BadRequest("ატვირთული ინფორმაცია არასწორია, " + errMessage);
            //}

            var (chpData, message) = this.CheckBody<ChangeProfileModel>();
            if (chpData == null)
                return BadRequest(message ?? "");

            var userName = HttpContext.User.Identity?.Name;
            //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
            var user = userName == null
                ? null
                : _userManager.FindByNameAsync(CurrentUserName).Result;
            //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
            if (user == null)
                return BadRequest("მომხმარებელი არ არსებობს");

            if (user.Id != chpData.Userid || user.UserName != chpData.UserName || user.Email != chpData.Email)
                return BadRequest(
                    "ვერ მოხერხდა მომხმარებლის იდენტიფიკაცია. მომხმარებლის ინფორმაციის შენახვა ვერ მოხერხდა");

            if (chpData.FirstName is null)
                return BadRequest("მომხმარებლის სახელი არასწორია");

            if (chpData.LastName is null)
                return BadRequest("მომხმარებლის გვარი არასწორია");

            user.FirstName = chpData.FirstName;
            user.LastName = chpData.LastName;
            var result = _userManager.UpdateAsync(user).Result;
            //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
            if (!result.Succeeded)
                return BadRequest("მომხმარებლის ინფორმაციის შენახვა ვერ მოხერხდა");

            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("მომხმარებლის ინფორმაციის შენახვისას მოხდა შეცდომა. ინფორმაციის შენახვა ვერ მოხერხდა!");
        }
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის პაროლის ცვლილება
    //შემავალი ინფორმაცია -> ChangePasswordModel კლასის ობიექტი
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> მოწმდება მიღებული ინფორმაციის ვალიდურობა და ხდება პაროლის ცვლილებების დაფიქსირება
    [HttpPut("changepassword")]
    public ActionResult ChangePassword()
    {
        try
        {
            ////მივიღოთ ინფორმაცია მიღებული შეტყობინების ტანიდან
            //string body;
            //using (StreamReader reader = new StreamReader(Request.Body)) 
            //  body = reader.ReadToEnd();

            ////Json გადავიყვანოთ მოსალოდნელ ობიექტში
            //ChangePasswordModel chpData = JsonConvert.DeserializeObject<ChangePasswordModel>(body);

            ////შევამოწმოთ მიღებული ინფორმაცია ვალიდურია თუ არა
            //if (!ModelState.IsValid)
            //  return BadRequest("გაგზავნილი ინფორმაცია არასწორია");

            var (chpData, message) = this.CheckBody<ChangePasswordModel>();
            if (chpData == null)
                return BadRequest(message ?? "");

            if (chpData.NewPassword != chpData.NewPasswordConfirm)
                return BadRequest("პაროლები არასწორია");

            //მოვძებნოთ მომხმარებელი მოწოდებული მომხმარებლის სახელით
            var user = _userManager.FindByNameAsync(chpData.UserName).Result;
            //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
            if (user == null)
                return BadRequest("მომხმარებელი არ არსებობს");

            if (CurrentUserName != user.UserName || CurrentUserName != chpData.UserName)
                return BadRequest("ვერ მოხერხდა მომხმარებლის იდენტიფიკაცია. პაროლი არ შეიცვალა");


            var result = _userManager.ChangePasswordAsync(user, chpData.OldPassword, chpData.NewPassword)
                .Result;
            //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
            if (!result.Succeeded)
                return BadRequest("პაროლის შეცვლა ვერ მოხერხდა");

            return Ok();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("პაროლის შეცვლისას მოხდა შეცდომა. პაროლის შეცვლა ვერ მოხერხდა!");
        }
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
    [HttpDelete("deletecurrentuser/{userName}")]
    public ActionResult DeleteCurrentUser(string userName)
    {
        //ეს ერთგვარი ტესტია. თუ კოდი აქამდე მოვიდა, მიმდინარე მომხმარებელი ვალიდურია
        if (CurrentUserName != userName)
            return BadRequest("არასწორი მოთხოვნა, მომხმარებლის წაშლა ვერ მოხერხდა");
        UsersMdRepo usersMdRepo = new(_userManager);
        var oldUser = _userManager.FindByNameAsync(userName).Result;
        if (usersMdRepo.Delete(oldUser.Id))
            return Ok();

        return BadRequest("წაშლისას მოხდა შეცდომა, მომხმარებლის წაშლა ვერ მოხერხდა");
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> მიმდინარე მომხმარებლის უფლებების შესაბამისი მენიუს შესახებ ინფორმაციის ჩატვირთვა
    //შემავალი ინფორმაცია -> არა
    //უფლება -> მხოლოდ ავტორიზაცია
    //მოქმედება -> რეპოზიტორიას გადაეწოდება მიმდინარე მომხმარებლის სახელი და
    //  მისი უფლებების მიხედვით ჩატვირთული მენიუს შესახებ ინფორმაციას უბრუნებს გამომძახებელს
    [HttpGet("getmainmenu")]
    public async Task<ActionResult<MainMenuModel>> GetMainMenu()
    {
        try
        {
            if (CurrentUserName is null)
                return BadRequest("მომხმარებელი არასწორია");
            var mainMenuModel = await _mdRepo.GetMainMenu(CurrentUserName);
            if (mainMenuModel == null)
                return BadRequest("მენიუს ჩატვირთვა ვერ მოხერხდა");
            return Ok(mainMenuModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error occurred executing {nameof(GetMainMenu)}.");
            return BadRequest($"Error occurred executing {nameof(GetMainMenu)}.");
        }
    }
}