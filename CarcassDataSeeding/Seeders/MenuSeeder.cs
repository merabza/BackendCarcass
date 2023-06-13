using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class MenuSeeder : AdvancedDataSeeder<MenuItm>
{
    public MenuSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
    }

    protected override bool CreateByJsonFile()
    {
        List<MenuItmSeederModel> seedData = LoadFromJsonFile<MenuItmSeederModel>();
        List<MenuItm> dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
        {
            return false;
        }

        DataSeederTempData.Instance.SaveIntIdKeys<MenuItm>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    private List<MenuItm> CreateListBySeedData(List<MenuItmSeederModel> menuSeedData)
    {
        DataSeederTempData tempData = DataSeederTempData.Instance;
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
        DataSeederTempData tempData = DataSeederTempData.Instance;

        MenuItm[] menuItems =
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