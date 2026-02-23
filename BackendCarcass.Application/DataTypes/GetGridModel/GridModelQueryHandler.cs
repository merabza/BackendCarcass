using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Repositories;
using BackendCarcassShared.BackendCarcassContracts.Errors;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.DataTypes.GetGridModel;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GridModelQueryHandler : IQueryHandler<GridModelRequestQuery, string>
{
    private readonly IMenuRightsRepository _repository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GridModelQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<string, Err[]>> Handle(GridModelRequestQuery request, CancellationToken cancellationToken)
    {
        string? res = await _repository.GridModel(request.GridName, cancellationToken);
        if (res == null)
        {
            return new[] { DataTypesApiErrors.GridNotFound(request.GridName) };
        }

        return res;
    }
}
