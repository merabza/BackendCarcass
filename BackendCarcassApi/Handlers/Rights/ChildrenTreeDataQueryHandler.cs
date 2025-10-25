using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Rights;
using CarcassDom;
using CarcassDom.Models;
using CarcassIdentity;
using CarcassMasterDataDom;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChildrenTreeDataQueryHandler : ICommandHandler<ChildrenTreeDataCommandRequest, List<DataTypeModel>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChildrenTreeDataQueryHandler(IRightsRepository repo, IReturnValuesRepository rvRepo,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
    }

    public async Task<OneOf<List<DataTypeModel>, Err[]>> Handle(ChildrenTreeDataCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var result = await rightsCollector.ChildrenTreeData(_currentUser.Name, request.DataTypeKey, request.ViewStyle,
            cancellationToken);
        return result.Match<OneOf<List<DataTypeModel>, Err[]>>(r => r, e => (Err[])e);
    }
}