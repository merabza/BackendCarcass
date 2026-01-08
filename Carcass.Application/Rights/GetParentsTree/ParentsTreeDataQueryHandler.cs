using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassIdentity;
using CarcassMasterData;
using CarcassRights;
using CarcassRights.Models;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.Rights.GetParentsTree;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ParentsTreeDataQueryHandler : IQueryHandler<ParentsTreeDataRequestQuery, List<DataTypeModel>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ParentsTreeDataQueryHandler(IRightsRepository repo, IReturnValuesRepository rvRepo, ICurrentUser currentUser)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
    }

    public async Task<OneOf<List<DataTypeModel>, Err[]>> Handle(ParentsTreeDataRequestQuery request,
        CancellationToken cancellationToken = default)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var result = await rightsCollector.ParentsTreeData(_currentUser.Name, request.ViewStyle, cancellationToken);

        return result.Match<OneOf<List<DataTypeModel>, Err[]>>(r => r, e => e.ToArray());
    }
}