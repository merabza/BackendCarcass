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

    protected override bool AdditionalCheck(List<MenuItmSeederModel> jsonData, List<MenuItm> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<MenuItm>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    protected override List<MenuItm> Adapt(List<MenuItmSeederModel> menuSeedData)
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

    protected override List<MenuItm> CreateListByRules()
    {
        var tempData = DataSeederTempData.Instance;

        var menuItems = new MenuItm[]
        {
            //carcass master data
            new()
            {
                MenKey = "DataTypes",
                MenName = "DataTypes - მონაცემთა ტიპები",
                MenValue = "dataTypes",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"),
                SortId = 7,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "Users",
                MenName = "მომხმარებლები",
                MenValue = "users",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"),
                SortId = 17,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "MenuEditor",
                MenName = "MenuEditor - მენიუს რედაქტორი",
                MenValue = "menu",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"),
                SortId = 4,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "MenuGroups",
                MenName = "MenuGroups - მენიუს ჯგუფები",
                MenValue = "menuGroups",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"),
                SortId = 4,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "Roles",
                MenName = "როლები",
                MenValue = "roles",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"),
                SortId = 0,
                MenLinkKey = "mdList"
            },

            //carcass
            new()
            {
                MenKey = "Rights",
                MenName = "უფლებები",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("Main"),
                SortId = 0,
                MenLinkKey = "Rights",
                MenIconName = "users-cog"
            },
            new()
            {
                MenKey = "CrudRightTypes",
                MenName = "მონაცემების ცვლილებაზე უფლებების ტიპები",
                MenValue = "crudRightTypes",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"),
                SortId = 0,
                MenLinkKey = "mdList"
            }
        };
        return menuItems.ToList();
    }
}