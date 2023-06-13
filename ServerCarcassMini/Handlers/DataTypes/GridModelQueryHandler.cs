using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassRepositories;
using MessagingAbstractions;
using OneOf;
using ServerCarcassMini.QueryRequests.DataTypes;
using SystemToolsShared;

namespace ServerCarcassMini.Handlers.DataTypes;

// ReSharper disable once UnusedType.Global
public sealed class GridModelQueryHandler : IQueryHandler<GridModelQueryRequest, string>
{
    private readonly IMenuRightsRepository _repository;

    public GridModelQueryHandler(IMenuRightsRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<string, IEnumerable<Err>>> Handle(GridModelQueryRequest request,
        CancellationToken cancellationToken)
    {
        var res = await _repository.GridModel(request.GridName);
        if (res == null)
            return new[] { DataTypesApiErrors.GridNotFound(request.GridName) };
        return res;
    }
}