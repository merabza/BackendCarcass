using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding.Seeders;

public sealed class UsersSeeder : DataSeeder<User>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly string _secretDataFolder;

    public UsersSeeder(UserManager<AppUser> userManager, string secretDataFolder, string dataSeedFolder,
        IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
        _secretDataFolder = secretDataFolder;
        _userManager = userManager;
    }

    protected override bool AdditionalCheck()
    {
        List<User> existingUsers = Repo.GetAll<User>();

        if (!GetAppUserModels().Select(userModel => new
                {
                    userModel,
                    existingUser = existingUsers.SingleOrDefault(sd =>
                        sd.NormalizedUserName == _userManager.NormalizeName(userModel.UserName)),
                    existingEmail =
                        existingUsers.SingleOrDefault(sd =>
                            sd.NormalizedEmail == _userManager.NormalizeEmail(userModel.Email))
                }).Where(w => w.existingUser == null && w.existingEmail == null).Select(s => s!.userModel)
                .All(CreateUser))
            return false;
        DataSeederTempData.Instance.SaveIntIdKeys<User>(Repo.GetAll<User>().ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    protected override bool CreateByJsonFile()
    {
        if (!Repo.CreateEntities(CreateListBySeedData(LoadFromJsonFile<UserSeederModel>())))
            return false;
        DataSeederTempData.Instance.SaveIntIdKeys<User>(Repo.GetAll<User>().ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    private List<User> CreateListBySeedData(List<UserSeederModel> usersSeedData)
    {
        return usersSeedData.Select(s => new User
        {
            UserName = s.UserName, NormalizedUserName = s.NormalizedUserName, Email = s.Email,
            NormalizedEmail = s.NormalizedEmail, PasswordHash = s.PasswordHash, FullName = s.FullName,
            FirstName = s.FirstName, LastName = s.LastName
        }).ToList();
    }

    private bool CreateUser(UserModel userModel)
    {
        //1. შევქმნათ ახალი მომხმარებელი
        AppUser user = new AppUser(userModel.UserName, userModel.FirstName, userModel.LastName)
            { Email = userModel.Email };
        IdentityResult result = _userManager.CreateAsync(user, userModel.Password).Result;
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (result.Succeeded)
            return true;
        Messages.AddRange(result.Errors.Select(s => s.Description));
        Messages.Add($"User {userModel.UserName} with email {userModel.Email} can not created.");
        return false;
    }

    private List<UserModel> GetAppUserModels()
    {
        return LoadFromJsonFile<UserModel>(_secretDataFolder, "Users.json").ToList();
    }
}