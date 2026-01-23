using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Database.Models;
using BackendCarcass.DataSeeding.Models;
using SystemTools.DatabaseToolsShared;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.DataSeeding.Seeders;

public /*open*/
    class AppClaimsSeeder : DataSeeder<AppClaim, AppClaimSeederModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AppClaimsSeeder(string dataSeedFolder, IDataSeederRepository repo, IUnitOfWork unitOfWork,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, unitOfWork, seedDataType, keyFieldNamesList)
    {
    }

    public override bool AdditionalCheck(List<AppClaimSeederModel> jsonData, List<AppClaim> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<AppClaim>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }
}
