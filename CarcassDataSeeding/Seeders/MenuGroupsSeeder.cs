using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using LanguageExt;
using SystemToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class MenuGroupsSeeder(string dataSeedFolder, IDataSeederRepository repo) : AdvancedDataSeeder<MenuGroup>(
        dataSeedFolder, repo)
{
    protected override Option<Err[]> CreateByJsonFile()
    {
        var seedData = LoadFromJsonFile<MenuGroupSeederModel>();
        var dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "MenuGroupEntitiesCannotBeCreated",
                    ErrorMessage = "MenuGroup entities cannot be created"
                }
            };

        DataSeederTempData.Instance.SaveIntIdKeys<MenuGroup>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    private static List<MenuGroup> CreateListBySeedData(List<MenuGroupSeederModel> menuGroupsSeedData)
    {
        return menuGroupsSeedData
            .Select(s => new MenuGroup { MengKey = s.MengKey, MengName = s.MengName, SortId = s.SortId }).ToList();
    }

    protected override List<MenuGroup> CreateMustList()
    {
        var menuGroups = new MenuGroup[]
        {
            //carcass
            new() { MengKey = "Main", MengName = "მთავარი", SortId = 0, Hidden = true },
            new() { MengKey = "MasterData", MengName = "ძირითადი ინფორმაცია", SortId = 200, Hidden = false },
        };

        return [.. menuGroups];
    }
}