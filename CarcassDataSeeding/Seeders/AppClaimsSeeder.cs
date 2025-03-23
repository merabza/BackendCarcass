using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class AppClaimsSeeder(string dataSeedFolder, IDataSeederRepository repo)
    : DataSeeder<AppClaim, AppClaimSeederModel>(dataSeedFolder, repo, ESeedDataType.RulesHasMorePriority)
{
    protected override bool AdditionalCheck(List<AppClaimSeederModel> seedData)
    {
        var dataList = Repo.GetAll<DataType>();
        DataSeederTempData.Instance.SaveIntIdKeys<AppClaim>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }
}