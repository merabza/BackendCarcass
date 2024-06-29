using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassDom;
using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using SystemToolsShared.Errors;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class CrudRightTypesSeeder(string dataSeedFolder, IDataSeederRepository repo) : AdvancedDataSeeder<CrudRightType>(
    dataSeedFolder, repo)
{
    protected override Option<Err[]> CreateByJsonFile()
    {
        var seedData = LoadFromJsonFile<CrudRightTypeSeederModel>();
        var dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
            return new Err[]
            {
                new()
                {
                    ErrorCode = "CrudRightTypeSEntitiesCannotBeCreated",
                    ErrorMessage = "CrudRightTypeS entities cannot be created"
                }
            };

        DataSeederTempData.Instance.SaveIntIdKeys<CrudRightType>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return null;
    }

    private static List<CrudRightType> CreateListBySeedData(
        IEnumerable<CrudRightTypeSeederModel> crudRightTypesSeedData)
    {
        return crudRightTypesSeedData.Select(s => new CrudRightType { CrtKey = s.CrtKey, CrtName = s.CrtName })
            .ToList();
    }

    protected override List<CrudRightType> CreateMustList()
    {
        CrudRightType[] crudRightTypes =
        [
            //თუ მონაცემთა ტიპზე არის უფლება, ჩავთვალოთ, რომ შესაბამის ცხრილზეც არის უფლება
            //ამიტომ ნახვის უფლებას ვაუქმებ
            new CrudRightType { CrtKey = nameof(ECrudOperationType.Create), CrtName = "შექმნა" },
            new CrudRightType { CrtKey = nameof(ECrudOperationType.Update), CrtName = "შეცვლა" },
            new CrudRightType { CrtKey = nameof(ECrudOperationType.Delete), CrtName = "წაშლა" }
            //new(nameof(ECrudOperationType.Confirm), "დადასტურება"),
        ];

        return [..crudRightTypes];
    }
}