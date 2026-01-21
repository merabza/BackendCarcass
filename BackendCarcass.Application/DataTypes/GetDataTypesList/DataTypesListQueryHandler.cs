using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Authentication;
using BackendCarcass.Identity;
using BackendCarcass.Repositories;
using BackendCarcassContracts.V1.Responses;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.DataTypes.GetDataTypesList;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DataTypesListQueryHandler : LoginCommandHandlerBase,
    IQueryHandler<DataTypesRequestQuery, DataTypesResponse[]>
{
    private readonly ICurrentUser _currentUser;
    private readonly IMenuRightsRepository _repository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypesListQueryHandler(IMenuRightsRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<OneOf<DataTypesResponse[], Err[]>> Handle(DataTypesRequestQuery request,
        CancellationToken cancellationToken)
    {
        DataTypesResponse[] res = await _repository.DataTypes(_currentUser.Name, cancellationToken);
        return res;
    }
}
