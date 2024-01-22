using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassDom;
using LanguageExt;
using SystemToolsShared;

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
        return crudRightTypesSeedData.Select(s => new CrudRightType(s.CrtKey, s.CrtName)).ToList();
    }

    protected override List<CrudRightType> CreateMustList()
    {
        CrudRightType[] crudRightTypes =
        {
            //თუ მონაცემთა ტიპზე არის უფლება, ჩავთვალოთ, რომ შესაბამის ცხრილზეც არის უფლება
            //ამიტომ ნახვის უფლებას ვაუქმებ
            new(nameof(ECrudOperationType.Create), "შექმნა"),
            new(nameof(ECrudOperationType.Update), "შეცვლა"),
            new(nameof(ECrudOperationType.Delete), "წაშლა"),
            //new(nameof(ECrudOperationType.Confirm), "დადასტურება"),
        };

        return crudRightTypes.ToList();
    }
}