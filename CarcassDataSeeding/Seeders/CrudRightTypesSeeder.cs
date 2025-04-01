using System.Collections.Generic;
using System.Linq;
using CarcassDataSeeding.Models;
using CarcassDb.Models;
using CarcassDom;
using DatabaseToolsShared;

namespace CarcassDataSeeding.Seeders;

public /*open*/
    class CrudRightTypesSeeder : DataSeeder<CrudRightType, CrudRightTypeSeederModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CrudRightTypesSeeder(string dataSeedFolder, IDataSeederRepository repo) : base(dataSeedFolder, repo,
        ESeedDataType.OnlyRules)
    {
    }

    protected override bool AdditionalCheck(List<CrudRightTypeSeederModel> seedData)
    {
        var dataList = Repo.GetAll<CrudRightType>();
        DataSeederTempData.Instance.SaveIntIdKeys<CrudRightType>(dataList.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    protected override List<CrudRightType> CreateListByRules()
    {
        CrudRightType[] crudRightTypes =
        [
            //თუ მონაცემთა ტიპზე არის უფლება, ჩავთვალოთ, რომ შესაბამის ცხრილზეც არის უფლება
            //ამიტომ ნახვის უფლებას ვაუქმებ
            new() { CrtKey = nameof(ECrudOperationType.Create), CrtName = "შექმნა" },
            new() { CrtKey = nameof(ECrudOperationType.Update), CrtName = "შეცვლა" },
            new() { CrtKey = nameof(ECrudOperationType.Delete), CrtName = "წაშლა" }
            //new(nameof(ECrudOperationType.Confirm), "დადასტურება"),
        ];

        return [..crudRightTypes];
    }
}