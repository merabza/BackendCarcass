using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Repositories;
using BackendCarcassShared.Contracts.Errors;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.DataTypes.GetMultipleGridModels;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MultipleGridModelsQueryHandler(IMenuRightsRepository repository)
    : IQueryHandler<MultipleGridModelsRequestQuery, Dictionary<string, string>>
{
    public async Task<OneOf<Dictionary<string, string>, Error[]>> Handle(MultipleGridModelsRequestQuery request,
        CancellationToken cancellationToken)
    {
        var resultList = new Dictionary<string, string>();

        //დამზადდეს ჩასატვირთი მოდელების შესაბამისი ცხრილების სახელების სია.
        //სიის დამზადება საჭიროა იმისათვის, რომ შესაძლებელი გახდეს მისი მეორედ გავლა
        //პირველი გავლისას მოწმდება უფლებები
        List<string?> gridNames = request.Grids.Where(w => !string.IsNullOrWhiteSpace(w)).Distinct().ToList();
        if (gridNames.Count == 0)
        {
            return new[] { DataTypesApiErrors.NoGridNamesInUriQuery };
        }

        List<Error> errors = [];
        //ხოლო მეორე გავლისას ხდება უშუალოდ საჭირო ინფორმაციის ჩატვირთვა
        foreach (string? gridName in gridNames)
        {
            string? res = await repository.GridModel(gridName!, cancellationToken);
            if (res != null)
            {
                resultList.Add(gridName!, res);
            }
            else
            {
                errors.Add(DataTypesApiErrors.GridNotFound(gridName!));
            }
        }

        if (errors.Count > 0)
        {
            return errors.ToArray();
        }

        return resultList;
    }
}
