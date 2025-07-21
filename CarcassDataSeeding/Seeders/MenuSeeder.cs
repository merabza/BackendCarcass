using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using DatabaseToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class MenuSeeder : DataSeeder<MenuItm, MenuItmSeederModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuSeeder(string dataSeedFolder, IDataSeederRepository repo,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, seedDataType, keyFieldNamesList)
    {
    }

    public override bool AdditionalCheck(List<MenuItmSeederModel> jsonData, List<MenuItm> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<MenuItm>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    public override List<MenuItm> Adapt(List<MenuItmSeederModel> menuSeedData)
    {
        var tempData = DataSeederTempData.Instance;
        return menuSeedData.Select(s => new MenuItm
        {
            MenGroupId = tempData.GetIntIdByKey<MenuGroup>(s.MenGroupIdMengKey),
            MenIconName = s.MenIconName,
            MenKey = s.MenKey,
            MenLinkKey = s.MenLinkKey,
            MenName = s.MenName,
            MenValue = s.MenValue,
            SortId = s.SortId
        }).ToList();
    }

    public override List<MenuItm> CreateListByRules()
    {
        var tempData = DataSeederTempData.Instance;
        const string mdList = nameof(mdList);

        var menuItems = new MenuItm[]
        {
            //carcass master data
            new()
            {
                MenKey = "DataTypes",
                MenName = "DataTypes - მონაცემთა ტიპები",
                MenValue = DataSeederRepo.GetTableName<DataType>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 7,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "Users",
                MenName = "მომხმარებლები",
                MenValue = DataSeederRepo.GetTableName<User>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 17,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "MenuEditor",
                MenName = "MenuEditor - მენიუს რედაქტორი",
                MenValue = DataSeederRepo.GetTableName<MenuItm>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 4,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "MenuGroups",
                MenName = "MenuGroups - მენიუს ჯგუფები",
                MenValue = DataSeederRepo.GetTableName<MenuGroup>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 4,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "Roles",
                MenName = "როლები",
                MenValue = DataSeederRepo.GetTableName<Role>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 0,
                MenLinkKey = mdList
            },

            //carcass
            new()
            {
                MenKey = "Rights",
                MenName = "უფლებები",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.Main),
                SortId = 0,
                MenLinkKey = "Rights",
                MenIconName = "users-cog"
            },
            new()
            {
                MenKey = "CrudRightTypes",
                MenName = "მონაცემების ცვლილებაზე უფლებების ტიპები",
                MenValue = DataSeederRepo.GetTableName<CrudRightType>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 0,
                MenLinkKey = mdList
            }
        };
        return menuItems.ToList();
    }
}