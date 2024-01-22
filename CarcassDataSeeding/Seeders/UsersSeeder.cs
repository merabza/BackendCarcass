using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using SystemToolsShared;

namespace CarcassDataSeeding.Seeders;

public sealed class UsersSeeder(
    UserManager<AppUser> userManager,
    string secretDataFolder,
    string dataSeedFolder,
    IDataSeederRepository repo) : DataSeeder<User>(dataSeedFolder, repo)
{
    protected override Option<Err[]> AdditionalCheck()
    {
        var existingUsers = Repo.GetAll<User>();

        var userToCreate = GetAppUserModels().Select(userModel => new
        {
            userModel,
            existingUser = existingUsers.SingleOrDefault(sd =>
                sd.NormalizedUserName == userManager.NormalizeName(userModel.UserName)),
            existingEmail =
                existingUsers.SingleOrDefault(sd =>
                    sd.NormalizedEmail == userManager.NormalizeEmail(userModel.Email))
        }).Where(w => w.existingUser == null && w.existingEmail == null).Select(s => s!.userModel);

        var userCreateErrors = new List<Err>();
        foreach (var userModel in userToCreate)
        {
            var result = CreateUser(userModel);
            if (result.IsSome)
            {
                userCreateErrors.AddRange((Err[])result);
            }
        }

        if (userCreateErrors.Count > 0)
            return userCreateErrors.ToArray();

        DataSeederTempData.Instance.SaveIntIdKeys<User>(Repo.GetAll<User>().ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    protected override Option<Err[]> CreateByJsonFile()
    {
        if (!Repo.CreateEntities(CreateListBySeedData(LoadFromJsonFile<UserSeederModel>())))
            return new Err[]
            {
                new() { ErrorCode = "UserEntitiesCannotBeCreated", ErrorMessage = "User entities cannot be created" }
            };
        DataSeederTempData.Instance.SaveIntIdKeys<User>(Repo.GetAll<User>().ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    private static List<User> CreateListBySeedData(List<UserSeederModel> usersSeedData)
    {
        return usersSeedData.Select(s => new User
        {
            UserName = s.UserName, NormalizedUserName = s.NormalizedUserName, Email = s.Email,
            NormalizedEmail = s.NormalizedEmail, PasswordHash = s.PasswordHash, FullName = s.FullName,
            FirstName = s.FirstName, LastName = s.LastName
        }).ToList();
    }

    private Option<Err[]> CreateUser(UserModel userModel)
    {
        //1. შევქმნათ ახალი მომხმარებელი
        var user = new AppUser(userModel.UserName, userModel.FirstName, userModel.LastName)
            { Email = userModel.Email };
        var result = userManager.CreateAsync(user, userModel.Password).Result;
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (result.Succeeded)
            return null;

        var errors = result.Errors.Select(s => new Err { ErrorCode = s.Code, ErrorMessage = s.Description }).ToList();
        errors.Add(new Err
        {
            ErrorCode = "UserWithThisEmailCanNotBeCreated",
            ErrorMessage = $"User {userModel.UserName} with email {userModel.Email} can not be created."
        });
        return errors.ToArray();
    }

    private List<UserModel> GetAppUserModels()
    {
        return LoadFromJsonFile<UserModel>(secretDataFolder, "Users.json").ToList();
    }
}