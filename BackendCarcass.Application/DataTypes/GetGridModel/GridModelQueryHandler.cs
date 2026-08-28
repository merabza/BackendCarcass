using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Repositories;
using BackendCarcassShared.Contracts.Errors;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.DataTypes.GetGridModel;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GridModelQueryHandler(IMenuRightsRepository repository)
    : IQueryHandler<GridModelRequestQuery, string>
{
    public async Task<Result<string>> Handle(GridModelRequestQuery request, CancellationToken cancellationToken)
    {
        string? res = await repository.GridModel(request.GridName, cancellationToken);
        if (res == null)
        {
            return Result.Failure<string>(DataTypesApiErrors.GridNotFound(request.GridName));
        }

        return res;
    }
}
