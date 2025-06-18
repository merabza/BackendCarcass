using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using DatabaseToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class MenuGroupsSeeder : DataSeeder<MenuGroup, MenuGroupSeederModel>
{
    public const string Main = nameof(Main);
    public const string MasterData = nameof(MasterData);

    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuGroupsSeeder(string dataSeedFolder, IDataSeederRepository repo,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, seedDataType, keyFieldNamesList)
    {
    }

    protected override bool AdditionalCheck(List<MenuGroupSeederModel> jsonData, List<MenuGroup> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<MenuGroup>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    protected override List<MenuGroup> CreateListByRules()
    {
        var menuGroups = new MenuGroup[]
        {
            //carcass
            new() { MengKey = Main, MengName = "მთავარი", SortId = 0, Hidden = true },
            new() { MengKey = MasterData, MengName = "ძირითადი ინფორმაცია", SortId = 200, Hidden = false }
        };

        return [.. menuGroups];
    }
}