using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using DatabaseToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class AppClaimsSeeder : DataSeeder<AppClaim, AppClaimSeederModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AppClaimsSeeder(string dataSeedFolder, IDataSeederRepository repo,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, seedDataType, keyFieldNamesList)
    {
    }

    public override bool AdditionalCheck(List<AppClaimSeederModel> jsonData, List<AppClaim> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<AppClaim>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }
}