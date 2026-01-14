using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassRepositories;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.DataTypes.GetGridModel;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GridModelQueryHandler : IQueryHandler<GridModelRequestQuery, string>
{
    private readonly IMenuRightsRepository _repository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GridModelQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<string, Err[]>> Handle(GridModelRequestQuery request,
        CancellationToken cancellationToken)
    {
        var res = await _repository.GridModel(request.GridName, cancellationToken);
        if (res == null)
        {
            return new[] { DataTypesApiErrors.GridNotFound(request.GridName) };
        }

        return res;
    }
}
