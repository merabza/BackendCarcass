using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Authentication;
using BackendCarcassShared.Contracts.V1.Responses;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using ICurrentUser = BackendCarcass.Application.Identity.ICurrentUser;
using IMenuRightsRepository = BackendCarcass.Application.Repositories.IMenuRightsRepository;

namespace BackendCarcass.Application.DataTypes.GetDataTypesList;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DataTypesListQueryHandler(IMenuRightsRepository repository, ICurrentUser currentUser)
    : LoginCommandHandlerBase, IQueryHandler<DataTypesRequestQuery, DataTypesResponse[]>
{
    public async Task<Result<DataTypesResponse[]>> Handle(DataTypesRequestQuery request,
        CancellationToken cancellationToken)
    {
        DataTypesResponse[] res = await repository.DataTypes(currentUser.Name, cancellationToken);
        return res;
    }
}
