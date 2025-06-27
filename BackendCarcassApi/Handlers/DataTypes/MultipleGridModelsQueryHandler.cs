using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.DataTypes;
using BackendCarcassContracts.Errors;
using CarcassRepositories;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.DataTypes;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    MultipleGridModelsQueryHandler : IQueryHandler<MultipleGridModelsQueryRequest, Dictionary<string, string>>
{
    private readonly IMenuRightsRepository _repository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MultipleGridModelsQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Dictionary<string, string>, IEnumerable<Err>>> Handle(
        MultipleGridModelsQueryRequest request, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> resultList = new();

        //დამზადდეს ჩასატვირთი მოდელების შესაბამისი ცხრილების სახელების სია.
        //სიის დამზადება საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მისი მეორედ გავლა
        //პირველი გავლისას მოწმდება უფლებები
        var gridNames = request.Grids.Where(w => !string.IsNullOrWhiteSpace(w)).Distinct().ToList();
        if (gridNames.Count == 0)
            return new[] { DataTypesApiErrors.NoGridNamesInUriQuery };

        List<Err> errors = [];
        //ხოლო მეორე გავლისას ხდება უშუალოდ საჭირო ინფორმაციის ჩატვირთვა
        foreach (var gridName in gridNames)
        {
            var res = await _repository.GridModel(gridName!, cancellationToken);
            if (res != null)
                resultList.Add(gridName!, res);
            else
                errors.Add(DataTypesApiErrors.GridNotFound(gridName!));
        }

        if (errors.Count > 0)
            return errors;
        return resultList;
    }
}