using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class MenuGroupsSeeder : AdvancedDataSeeder<MenuGroup>
{
    public MenuGroupsSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
    }

    protected override bool CreateByJsonFile()
    {
        List<MenuGroupSeederModel> seedData = LoadFromJsonFile<MenuGroupSeederModel>();
        List<MenuGroup> dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
        {
            return false;
        }

        DataSeederTempData.Instance.SaveIntIdKeys<MenuGroup>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    private List<MenuGroup> CreateListBySeedData(List<MenuGroupSeederModel> menuGroupsSeedData)
    {
        return menuGroupsSeedData
            .Select(s => new MenuGroup { MengKey = s.MengKey, MengName = s.MengName, SortId = s.SortId }).ToList();
    }

    protected override List<MenuGroup> CreateMustList()
    {
        MenuGroup[] menuGroups =
        {
            //carcass
            new() { MengKey = "Main", MengName = "მთავარი", SortId = 0, Hidden = true },
            new() { MengKey = "MasterData", MengName = "ძირითადი ინფორმაცია", SortId = 200, Hidden = false },
        };

        return menuGroups.ToList();
    }
}