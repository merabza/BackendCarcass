using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Authentication;
using BackendCarcass.Identity;
using BackendCarcass.Repositories;
using BackendCarcassShared.Contracts.V1.Responses;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.DataTypes.GetDataTypesList;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DataTypesListQueryHandler(IMenuRightsRepository repository, ICurrentUser currentUser)
    : LoginCommandHandlerBase, IQueryHandler<DataTypesRequestQuery, DataTypesResponse[]>
{
    public async Task<OneOf<DataTypesResponse[], Error[]>> Handle(DataTypesRequestQuery request,
        CancellationToken cancellationToken)
    {
        DataTypesResponse[] res = await repository.DataTypes(currentUser.Name, cancellationToken);
        return res;
    }
}
