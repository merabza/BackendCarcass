using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using LanguageExt;
using SystemToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class AppClaimsSeeder(string dataSeedFolder, IDataSeederRepository repo) : AdvancedDataSeeder<AppClaim>(
    dataSeedFolder, repo)
{
    protected override Option<Err[]> CreateByJsonFile()
    {
        var seedData = LoadFromJsonFile<AppClaimSeederModel>();
        var dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "AppClaimEntitiesCannotBeCreated", ErrorMessage = "AppClaim entities cannot be created"
                }
            };

        DataSeederTempData.Instance.SaveIntIdKeys<AppClaim>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    private static List<AppClaim> CreateListBySeedData(List<AppClaimSeederModel> appClaimsSeedData)
    {
        return appClaimsSeedData.Select(s => new AppClaim { AclKey = s.AclKey, AclName = s.AclName }).ToList();
    }

    protected override List<AppClaim> CreateMustList()
    {
        //AppClaim[] appClaims =
        //{
        //  new AppClaim {AclKey = "Confirm", AclName = "დადასტურება"},
        //  new AppClaim {AclKey = "SaveSamples", AclName = "ნიმუშების შენახვა"}
        //};

        //return appClaims.ToList();
        return new List<AppClaim>();
    }
}