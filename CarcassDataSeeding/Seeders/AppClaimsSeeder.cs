﻿using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using DatabaseToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class AppClaimsSeeder : DataSeeder<AppClaim, AppClaimSeederModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AppClaimsSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo,
        ESeedDataType.RulesHasMorePriority)
    {
    }

    protected override bool AdditionalCheck(List<AppClaimSeederModel> seedData)
    {
        var dataList = Repo.GetAll<DataType>();
        DataSeederTempData.Instance.SaveIntIdKeys<AppClaim>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }
}