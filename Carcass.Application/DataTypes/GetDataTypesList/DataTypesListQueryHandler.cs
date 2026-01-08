using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.V1.Responses;
using Carcass.Application.Authentication;
using CarcassIdentity;
using CarcassRepositories;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.DataTypes.GetDataTypesList;

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
        CancellationToken cancellationToken = default)
    {
        var res = await _repository.DataTypes(_currentUser.Name, cancellationToken);
        return res;
    }
}