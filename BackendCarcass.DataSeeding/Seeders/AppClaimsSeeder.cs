using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Database.Models;
using BackendCarcass.DataSeeding.Models;
using SystemTools.DatabaseToolsShared;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.DataSeeding.Seeders;

public /*open*/
    class AppClaimsSeeder : DataSeeder<AppClaim, AppClaimSeederModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AppClaimsSeeder(string dataSeedFolder, IDataSeederRepository repo, IDatabaseAbstraction databaseAbstraction,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, databaseAbstraction, seedDataType, keyFieldNamesList)
    {
    }

    public override bool AdditionalCheck(List<AppClaimSeederModel> jsonData, List<AppClaim> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<AppClaim>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }
}
