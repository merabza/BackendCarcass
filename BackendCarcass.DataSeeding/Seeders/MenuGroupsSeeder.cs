using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Database.Models;
using BackendCarcass.DataSeeding.Models;
using SystemTools.DatabaseToolsShared;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.DataSeeding.Seeders;

public /*open*/
    class MenuGroupsSeeder : DataSeeder<MenuGroup, MenuGroupSeederModel>
{
    public const string Main = nameof(Main);
    public const string MasterData = nameof(MasterData);

    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuGroupsSeeder(string dataSeedFolder, IDataSeederRepository repo, IUnitOfWork unitOfWork,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, unitOfWork, seedDataType, keyFieldNamesList)
    {
    }

    public override bool AdditionalCheck(List<MenuGroupSeederModel> jsonData, List<MenuGroup> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<MenuGroup>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    public override List<MenuGroup> CreateListByRules()
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
