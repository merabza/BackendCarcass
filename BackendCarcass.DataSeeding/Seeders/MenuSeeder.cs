using System.Collections.Generic;
using System.Linq;
using BackendCarcass.DataSeeding.Models;
using BackendCarcassDomain.Entities.CrudRightTypes;
using BackendCarcassDomain.Entities.DataTypes;
using BackendCarcassDomain.Entities.MenuGroups;
using BackendCarcassDomain.Entities.MenuItems;
using BackendCarcassDomain.Entities.Roles;
using BackendCarcassDomain.Entities.Users;
using SystemTools.DatabaseToolsShared;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.DataSeeding.Seeders;

public /*open*/
    class MenuSeeder : DataSeeder<MenuItem, MenuItmSeederModel>
{
    protected readonly IDatabaseAbstraction DatabaseAbstraction;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuSeeder(string dataSeedFolder, IDataSeederRepository repo, IDatabaseAbstraction databaseAbstraction,
        ESeedDataType seedDataType = ESeedDataType.OnlyDatabase, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, databaseAbstraction, seedDataType, keyFieldNamesList)
    {
        DatabaseAbstraction = databaseAbstraction;
    }

    public override bool AdditionalCheck(List<MenuItmSeederModel> jsonData, List<MenuItem> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<MenuItem>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    public override List<MenuItem> Adapt(List<MenuItmSeederModel> menuSeedData)
    {
        var tempData = DataSeederTempData.Instance;
        return
        [
            .. menuSeedData.Select(s => new MenuItem
            {
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(s.MenGroupIdMengKey),
                MenIconName = s.MenIconName,
                MenKey = s.MenKey,
                MenLinkKey = s.MenLinkKey,
                MenName = s.MenName,
                MenValue = s.MenValue,
                SortId = s.SortId
            })
        ];
    }

    public override List<MenuItem> CreateListByRules()
    {
        var tempData = DataSeederTempData.Instance;
        const string mdList = nameof(mdList);

        var menuItems = new MenuItem[]
        {
            //carcass master data
            new()
            {
                MenKey = "DataTypes",
                MenName = "DataTypes - მონაცემთა ტიპები",
                MenValue = DatabaseAbstraction.GetTableName<DataType>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 7,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "Users",
                MenName = "მომხმარებლები",
                MenValue = DatabaseAbstraction.GetTableName<User>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 17,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "MenuEditor",
                MenName = "MenuEditor - მენიუს რედაქტორი",
                MenValue = DatabaseAbstraction.GetTableName<MenuItem>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 4,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "MenuGroups",
                MenName = "MenuGroups - მენიუს ჯგუფები",
                MenValue = DatabaseAbstraction.GetTableName<MenuGroup>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 4,
                MenLinkKey = mdList
            },
            new()
            {
                MenKey = "Roles",
                MenName = "როლები",
                MenValue = DatabaseAbstraction.GetTableName<Role>(),
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
                MenValue = DatabaseAbstraction.GetTableName<CrudRightType>(),
                MenGroupId = tempData.GetIntIdByKey<MenuGroup>(MenuGroupsSeeder.MasterData),
                SortId = 0,
                MenLinkKey = mdList
            }
        };
        return [.. menuItems];
    }
}
