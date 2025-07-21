using System;
using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassMasterDataDom.Models;
using DatabaseToolsShared;
using Microsoft.AspNetCore.Identity;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class UsersSeeder : DataSeeder<User, UserSeederModel>
{
    private readonly string _secretDataFolder;
    private readonly UserManager<AppUser> _userManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UsersSeeder(UserManager<AppUser> userManager, string secretDataFolder, string dataSeedFolder,
        IDataSeederRepository repo, ESeedDataType seedDataType = ESeedDataType.OnlyJson,
        List<string>? keyFieldNamesList = null) : base(dataSeedFolder, repo, seedDataType, keyFieldNamesList)
    {
        _userManager = userManager;
        _secretDataFolder = secretDataFolder;
    }

    public override bool AdditionalCheck(List<UserSeederModel> jsonData, List<User> savedData)
    {
        var userToCreate = GetAppUserModels().Select(userModel => new
        {
            userModel,
            existingUser =
                savedData.SingleOrDefault(sd =>
                    sd.NormalizedUserName == _userManager.NormalizeName(userModel.UserName)),
            existingEmail =
                savedData.SingleOrDefault(sd => sd.NormalizedEmail == _userManager.NormalizeEmail(userModel.Email))
        }).Where(w => w.existingUser == null && w.existingEmail == null).Select(s => s.userModel);

        if (userToCreate.Any(userModel => !CreateUser(userModel)))
            return false;

        DataSeederTempData.Instance.SaveIntIdKeys<User>(DataSeederRepo.GetAll<User>()
            .ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    public override List<User> Adapt(List<UserSeederModel> usersSeedData)
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
        var result = _userManager.CreateAsync(user, userModel.Password).Result;
        //თუ ახალი მომხმარებლის შექმნისას წარმოიშვა პრობლემა, ვჩერდებით
        if (result.Succeeded)
            return true;

        throw new Exception($"User {userModel.UserName} with email {userModel.Email} can not be created.");
    }

    private List<UserModel> GetAppUserModels()
    {
        return [.. LoadFromJsonFile<UserModel>(_secretDataFolder, "Users.json")];
    }
}