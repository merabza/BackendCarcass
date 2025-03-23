using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding.Seeders;

public sealed class UsersSeeder(
    UserManager<AppUser> userManager,
    string secretDataFolder,
    string dataSeedFolder,
    IDataSeederRepository repo) : DataSeeder<User, UserSeederModel>(dataSeedFolder, repo)
{
    protected override bool AdditionalCheck(List<UserSeederModel> jMos)
    {
        var existingUsers = Repo.GetAll<User>();

        var userToCreate = GetAppUserModels().Select(userModel => new
        {
            userModel,
            existingUser =
                existingUsers.SingleOrDefault(sd =>
                    sd.NormalizedUserName == userManager.NormalizeName(userModel.UserName)),
            existingEmail =
                existingUsers.SingleOrDefault(sd =>
                    sd.NormalizedEmail == userManager.NormalizeEmail(userModel.Email))
        }).Where(w => w.existingUser == null && w.existingEmail == null).Select(s => s.userModel);

        if (userToCreate.Any(userModel => !CreateUser(userModel)))
            return false;

        DataSeederTempData.Instance.SaveIntIdKeys<User>(Repo.GetAll<User>().ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    protected override List<User> Adapt(List<UserSeederModel> usersSeedData)
    {
        return usersSeedData.Select(s => new User
        {
            UserName = s.UserName,
            NormalizedUserName = s.NormalizedUserName,
            Email = s.Email,
            NormalizedEmail = s.NormalizedEmail,
            PasswordHash = s.PasswordHash,
            FullName = s.FullName,
            FirstName = s.FirstName,
            LastName = s.LastName
        }).ToList();
    }

    private bool CreateUser(UserModel userModel)
    {
        //1. შევქმნათ ახალი მომხმარებელი
        var user = new AppUser(userModel.UserName, userModel.FirstName, userModel.LastName) { Email = userModel.Email };
        var result = userManager.CreateAsync(user, userModel.Password).Result;
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (result.Succeeded)
            return true;

        throw new Exception($"User {userModel.UserName} with email {userModel.Email} can not be created.");
    }

    private List<UserModel> GetAppUserModels()
    {
        return [.. LoadFromJsonFile<UserModel>(secretDataFolder, "Users.json")];
    }
}