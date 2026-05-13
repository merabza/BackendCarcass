using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Repositories;
using BackendCarcassShared.Contracts.Errors;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.DataTypes.GetGridModel;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GridModelQueryHandler(IMenuRightsRepository repository)
    : IQueryHandler<GridModelRequestQuery, string>
{
    public async Task<OneOf<string, Error[]>> Handle(GridModelRequestQuery request, CancellationToken cancellationToken)
    {
        string? res = await repository.GridModel(request.GridName, cancellationToken);
        if (res == null)
        {
            return new[] { DataTypesApiErrors.GridNotFound(request.GridName) };
        }

        return res;
    }
}
