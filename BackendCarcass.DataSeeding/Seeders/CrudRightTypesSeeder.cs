using System.Collections.Generic;
using System.Linq;
using BackendCarcass.Database.Models;
using BackendCarcass.DataSeeding.Models;
using SystemTools.DatabaseToolsShared;
using SystemTools.DomainShared.Repositories;

namespace BackendCarcass.DataSeeding.Seeders;

public /*open*/
    class CrudRightTypesSeeder : DataSeeder<CrudRightType, CrudRightTypeSeederModel>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CrudRightTypesSeeder(string dataSeedFolder, IDataSeederRepository repo, IUnitOfWork unitOfWork,
        ESeedDataType seedDataType = ESeedDataType.OnlyJson, List<string>? keyFieldNamesList = null) : base(
        dataSeedFolder, repo, unitOfWork, seedDataType, keyFieldNamesList)
    {
    }

    public override bool AdditionalCheck(List<CrudRightTypeSeederModel> jsonData, List<CrudRightType> savedData)
    {
        DataSeederTempData.Instance.SaveIntIdKeys<CrudRightType>(savedData.ToDictionary(k => k.Key, v => v.Id));
        return true;
    }

    public override List<CrudRightType> CreateListByRules()
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
