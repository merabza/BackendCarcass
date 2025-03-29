using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using DatabaseToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class MenuGroupsSeeder(string dataSeedFolder, IDataSeederRepository repo)
    : DataSeeder<MenuGroup, MenuGroupSeederModel>(dataSeedFolder, repo, ESeedDataType.OnlyRules)
{
    protected override bool AdditionalCheck(List<MenuGroupSeederModel> jMos)
    {
        var dataList = Repo.GetAll<MenuGroup>();
        DataSeederTempData.Instance.SaveIntIdKeys<MenuGroup>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    protected override List<MenuGroup> CreateListByRules()
    {
        var menuGroups = new MenuGroup[]
        {
            //carcass
            new() { MengKey = "Main", MengName = "მთავარი", SortId = 0, Hidden = true },
            new() { MengKey = "MasterData", MengName = "ძირითადი ინფორმაცია", SortId = 200, Hidden = false }
        };

        return [.. menuGroups];
    }
}