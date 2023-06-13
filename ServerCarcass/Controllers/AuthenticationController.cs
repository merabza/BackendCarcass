using System;
using System.Collections.Generic;
using System.Linq;
using CarcassIdentity;
using CarcassIdentity.Models;
using CarcassJsonModels;
using CarcassRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ServerCarcass.Controllers;

//კონტროლერი -> გამოიყენება ახალი მომხმარებლის დასარეგისტრირებლად და არსებული მომხმარებლების ავტორიზაციისათვის
//ორივე ეს ქმედება ხდება ავტორიზაციამდე, ამიტომ აქ ავტორიზაცია მოთხოვნილი არ არის
[ApiController]
[Route("api/[controller]")]
public sealed class AuthenticationController : Controller //რეფაქტორინგი გაკეთებულია
{
    private static int _lastSequentialNumber;
    private readonly IOptions<IdentitySettings> _identitySettings;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IMasterDataRepository _mdRepo;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthenticationController(IMasterDataRepository mdRepo, IOptions<IdentitySettings> settings,
        UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr, ILogger<AuthenticationController> logger)
    {
        _mdRepo = mdRepo;
        _userManager = userMgr;
        _signInManager = signinMgr;
        _identitySettings = settings;
        _logger = logger;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> დაარეგისტრიროს ახალი მომხმარებელი ბაზაში
    //შემავალი ინფორმაცია -> RegistrationModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს ახალი მომხმარებლის დარეგსიტრირებას
    //   და თუ რეგისტრაცია წარმატებით დასრულდა ავტომატურად ალოგინებს ახალ მომხმარებელს.
    //   გამოდის რომ ახალ მომხმარებელს ეგრევე შეუძლია მუშაობის დაწყება.
    //   მაგრამ სამწუხაროდ უფლებების არქონის გამო პრაქტიკულად შეეძლება მხოლოდ თავისი ინფორმაციის ცვლილება
    //   ან თავისივე რეგისტრაციის წაშლა
    [HttpPost("registration")]
    public ActionResult<AppUserModel> Registration()
    {
        try
        {
            ////მივიღოთ ინფორმაცია მიღებული შეტყობინების ტანიდან
            //string body;
            //using (var reader = new StreamReader(Request.Body))
            //{
            //  body = reader.ReadToEnd();
            //}
            ////Json გადავიყვანოთ მოსალოდნელ ობიექტში
            //RegistrationModel regData = JsonConvert.DeserializeObject<RegistrationModel>(body);

            ////შევამოწმოთ მიღებული ინფორმაცია ვალიდურია თუ არა
            //if (!TryValidateModel(regData, nameof(RegistrationModel)))
            //{
            //  string errMessage =
            //    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(s => s.ErrorMessage));
            //  return BadRequest("ატვირთული ინფორმაცია არასწორია, " + errMessage);
            //}

            var (regData, message) = this.CheckBody<RegistrationModel>();
            if (regData == null)
                return BadRequest(message ?? "");


            if (regData.Password != regData.ConfirmPassword)
                return BadRequest("პაროლები არასწორია");

            if (regData.UserName is null) return BadRequest("მომხმარებლის სახელი შევსებული არ არის");

            if (regData.FirstName is null) return BadRequest("სახელი შევსებული არ არის");

            if (regData.LastName is null) return BadRequest("გვარი შევსებული არ არის");

            if (regData.Password is null) return BadRequest("პაროლი შევსებული არ არის");

            //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
            var user = _userManager.FindByNameAsync(regData.UserName).Result;
            //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
            if (user != null)
                return BadRequest("მომხმარებელი ასეთი სახელით უკვე არსებობს");

            //მოწოდებული მომხმარებლის სახელით ხომ არ არსებობს უკვე რომელიმე მომხმარებელი
            user = _userManager.FindByEmailAsync(regData.Email).Result;
            //თუ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
            if (user != null)
                return BadRequest("მომხმარებელი ასეთი ელექტრონული ფოსტით უკვე არსებობს");

            //1. შევქმნათ ახალი მომხმარებელი
            user = new AppUser(regData.UserName, regData.FirstName, regData.LastName) { Email = regData.Email };
            var result = _userManager.CreateAsync(user, regData.Password).Result;
            //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
            if (!result.Succeeded)
                return BadRequest("პაროლის გამოყენება ვერ მოხერხდა, საჭიროა უფრო რთული პაროლი");

            //აქ მოვალთ მხოლოდ იმ შემთხვევაში, თუ მომხმარებელი წარმატებით შეიქმნა,
            //მოხდეს მისი ავტომატური ავტორიზაცია
            user = Login(user, regData.Password);
            if (user == null)
                return BadRequest(new { message = "ახალი მომხმარებლის შექმნა ვერ მოხერხდა" });

            //ახლადშექმნილ მომხმარებელს როლები არ აქვს, ამიტომ შემდეგი ბრძანება დაკომენტარებულია
            //თუ მომავალში საჭირო გახდა, რომ ახლადშექმნილ მომხმარებელს ავტომატურად მიეცეს როლი, მაშინ შემდეგი ბრძანება უნდა აღსდგეს
            //IList<string> roles = _userManager.GetRolesAsync(user).Result;

            if (_identitySettings.Value.JwtSecret is null)
                return BadRequest(new { message = "პარამეტრები არასწორია" });

            _lastSequentialNumber++;
            AppUserModel appUserModel = new(user.Id, _lastSequentialNumber, user.UserName, user.Email,
                user.CreateJwToken(_identitySettings.Value.JwtSecret, _lastSequentialNumber), user.FirstName,
                user.LastName,
                "");
            return Ok(appUserModel);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("რეგისტრაციის პროცესის შესრულებისას მოხდა შეცდომა. რეგისტრაცია ვერ მოხერხდა!");
        }
    }


    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> არსებული მომხმარებლის ავტორიზაცია პაროლის გამოყენებით
    //შემავალი ინფორმაცია -> LoginModel კლასის ობიექტი, რომელიც მოდის ვებიდან
    //მოქმედება -> სხვადასხვა შემოწმებების შემდეგ ცდილობს მომხმარებლის ავტორიზებას
    //   წარმატებული ავტორიზების შემთხვევაში იქმნება JwT, რომელიც მომხმარებლის ინფორმაციასთან ერთად გადაეწოდება გამომძახებელს
    // POST api/authentication/login
    [HttpPost("login")]
    public ActionResult<AppUserModel> Login()
    {
        try
        {
            //string body;
            //using (StreamReader reader = new StreamReader(Request.Body))
            //  body = reader.ReadToEnd();

            //LoginModel userParam = JsonConvert.DeserializeObject<LoginModel>(body);

            ////შევამოწმოთ მიღებული ინფორმაცია ვალიდურია თუ არა
            //if (!TryValidateModel(userParam, nameof(LoginModel)))
            //{
            //  string errMessage =
            //    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(s => s.ErrorMessage));
            //  return BadRequest("გაგზავნილი ინფორმაცია არასწორია, " + errMessage);
            //}

            var (userParam, message) = this.CheckBody<LoginModel>();
            if (userParam == null)
                return BadRequest(message ?? "");

            if (userParam.Password is null) return BadRequest("პაროლი შევსებული არ არის");

            var user = _userManager.FindByNameAsync(userParam.UserName).Result;

            user = Login(user, userParam.Password);

            if (user == null)
                return Unauthorized("მომხმარებლის სახელი ან პაროლი არასწორია.");

            IList<string> roles = _userManager.GetRolesAsync(user).Result;

            if (_identitySettings.Value.JwtSecret is null)
                return BadRequest(new { message = "პარამეტრები არასწორია" });

            _lastSequentialNumber++;
            AppUserModel appUserModel = new(user.Id, _lastSequentialNumber, user.UserName, user.Email,
                user.CreateJwToken(_identitySettings.Value.JwtSecret, _lastSequentialNumber),
                roles.Aggregate("", (cur, next) => cur + (cur == "" ? "" : ", ") + next), user.FirstName, user.LastName,
                _mdRepo.GetUserAppClaims(user.UserName));
            return Ok(appUserModel);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
            return BadRequest("ავტორიზაციის პროცესის შესრულებისას მოხდა შეცდომა. ავტორიზაცია ვერ მოხერხდა!");
        }
    }

    //ამ მეთოდით ავტორიზაციის პროცესი გამოტანილია ცალკე
    //და გამოიყენება როგორც ავტორიზაციისას, ისე ახალი მომხმარებლის დარეგისტრირებისას,
    //რომ ავტომატურად მოხდეს რეგისტრაციისას ავტორიზაციაც
    private AppUser? Login(AppUser? user, string password)
    {
        if (user == null)
            return null;
        _signInManager.SignOutAsync().Wait();
        var result =
            _signInManager.PasswordSignInAsync(user, password, true, false).Result;
        return result.Succeeded ? user : null;
    }
}