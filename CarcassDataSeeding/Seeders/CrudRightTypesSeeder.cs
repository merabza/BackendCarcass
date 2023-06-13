using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassRightsDom;

namespace CarcassDataSeeding.Seeders;

public /*open*/ class CrudRightTypesSeeder : AdvancedDataSeeder<CrudRightType>
{
    public CrudRightTypesSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo)
    {
    }

    protected override bool CreateByJsonFile()
    {
        var seedData = LoadFromJsonFile<CrudRightTypeSeederModel>();
        var dataList = CreateListBySeedData(seedData);
        if (!Repo.CreateEntities(dataList))
        {
            return false;
        }

        DataSeederTempData.Instance.SaveIntIdKeys<CrudRightType>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
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