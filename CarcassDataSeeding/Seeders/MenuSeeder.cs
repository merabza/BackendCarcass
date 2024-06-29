using CarcassDataSeeding.Models;
using CarcassDb.Models;
using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using SystemToolsShared.Errors;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class MenuSeeder(string dataSeedFolder, IDataSeederRepository repo) : AdvancedDataSeeder<MenuItm>(dataSeedFolder,
    repo)
{
    protected override Option<Err[]> CreateByJsonFile()
    {
        var seedData = LoadFromJsonFile<MenuItmSeederModel>();
        var dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
            return new Err[]
            {
                new() { ErrorCode = "MenuEntitiesCannotBeCreated", ErrorMessage = "Menu entities cannot be created" }
            };
        DataSeederTempData.Instance.SaveIntIdKeys<MenuItm>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    private static List<MenuItm> CreateListBySeedData(List<MenuItmSeederModel> menuSeedData)
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

    protected override List<MenuItm> CreateMustList()
    {
        var tempData = DataSeederTempData.Instance;

        var menuItems = new MenuItm[]
        {
            //carcass master data
            new()
            {
                MenKey = "DataTypes", MenName = "DataTypes - მონაცემთა ტიპები", MenValue = "dataTypes",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"), SortId = 7,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "Users", MenName = "მომხმარებლები", MenValue = "users",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"), SortId = 17,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "MenuEditor", MenName = "MenuEditor - მენიუს რედაქტორი", MenValue = "menu",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"), SortId = 4,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "MenuGroups", MenName = "MenuGroups - მენიუს ჯგუფები", MenValue = "menuGroups",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"), SortId = 4,
                MenLinkKey = "mdList"
            },
            new()
            {
                MenKey = "Roles", MenName = "როლები", MenValue = "roles",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"), SortId = 0,
                MenLinkKey = "mdList"
            },


            //carcass
            new()
            {
                MenKey = "Rights", MenName = "უფლებები", MenGroupId = tempData.GetIntIdByKey<MenuGroup>("Main"),
                SortId = 0,
                MenLinkKey = "Rights", MenIconName = "users-cog"
            },
            new()
            {
                MenKey = "CrudRightTypes", MenName = "მონაცემების ცვლილებაზე უფლებების ტიპები",
                MenValue = "crudRightTypes",
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>("MasterData"), SortId = 0,
                MenLinkKey = "mdList"
            }
        };
        return menuItems.ToList();
    }
}