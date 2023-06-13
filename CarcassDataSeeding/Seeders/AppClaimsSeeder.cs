using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class AppClaimsSeeder : AdvancedDataSeeder<AppClaim>
{
    public AppClaimsSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
    }

    protected override bool CreateByJsonFile()
    {
        List<AppClaimSeederModel> seedData = LoadFromJsonFile<AppClaimSeederModel>();
        List<AppClaim> dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
        {
            return false;
        }

        DataSeederTempData.Instance.SaveIntIdKeys<AppClaim>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    private List<AppClaim> CreateListBySeedData(List<AppClaimSeederModel> appClaimsSeedData)
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