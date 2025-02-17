using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.Rights;
using CarcassDom;
using CarcassDom.Models;
using CarcassIdentity;
using CarcassMasterDataDom;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ParentsTreeDataQueryHandler : IQueryHandler<ParentsTreeDataQueryRequest, List<DataTypeModel>>
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

    public async Task<OneOf<List<DataTypeModel>, IEnumerable<Err>>> Handle(ParentsTreeDataQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var result = await rightsCollector.ParentsTreeData(_currentUser.Name, request.ViewStyle, cancellationToken);

        return result.Match<OneOf<List<DataTypeModel>, IEnumerable<Err>>>(r => r, e => (Err[])e);
    }
}