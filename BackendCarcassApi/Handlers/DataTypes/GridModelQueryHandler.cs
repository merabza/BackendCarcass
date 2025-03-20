using System.Collections.Generic;
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
public sealed class GridModelQueryHandler : IQueryHandler<GridModelQueryRequest, string>
{
    private readonly IMenuRightsRepository _repository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GridModelQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<string, IEnumerable<Err>>> Handle(GridModelQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var res = await _repository.GridModel(request.GridName, cancellationToken);
        if (res == null)
            return new[] { DataTypesApiErrors.GridNotFound(request.GridName) };
        return res;
    }
}