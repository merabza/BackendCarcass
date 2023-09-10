using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.DataTypes;
using CarcassContracts.ErrorModels;
using CarcassRepositories;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.DataTypes;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    MultipleGridModelsQueryHandler : IQueryHandler<MultipleGridModelsQueryRequest, Dictionary<string, string>>
{
    private readonly IMenuRightsRepository _repository;

    public MultipleGridModelsQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Dictionary<string, string>, IEnumerable<Err>>> Handle(
        MultipleGridModelsQueryRequest request, CancellationToken cancellationToken)
    {
        Dictionary<string, string> resultList = new();
        //შემოწმდეს მოწოდებული მოთხოვნა
        var reqQuery = request.HttpRequest.Query["grids"];
        if (reqQuery.Count == 0)
            return new[] { DataTypesApiErrors.NoGridNamesInUriQuery };

        //დამზადდეს ჩასატვირთი მოდელების შესაბამისი ცხრილების სახელების სია.
        //სიის დამზადება საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მისი მეორედ გავლა
        //პირველი გავლისას მოწმდება უფლებები
        var tableNames = reqQuery.Distinct().ToList();

        //ხოლო მეორე გავლისას ხდება უშუალოდ საჭირო ინფორმაციის ჩატვირთვა
        foreach (var tableName in tableNames.Where(tableName => tableName is not null))
        {
            var res = await _repository.GridModel(tableName!, cancellationToken);
            if (res == null)
                return new[] { DataTypesApiErrors.GridNotFound(tableName!) };
            resultList.Add(tableName!, res);
        }

        return resultList;
    }
}