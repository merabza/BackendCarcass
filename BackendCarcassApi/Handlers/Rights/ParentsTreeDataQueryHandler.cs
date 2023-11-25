using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.Rights;
using CarcassDom;
using CarcassDom.Models;
using CarcassMasterDataDom;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ParentsTreeDataQueryHandler : IQueryHandler<ParentsTreeDataQueryRequest, List<DataTypeModel>>
{
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;

    public ParentsTreeDataQueryHandler(IRightsRepository repo, IReturnValuesRepository rvRepo)
    {
        _repo = repo;
        _rvRepo = rvRepo;
    }

    public async Task<OneOf<List<DataTypeModel>, IEnumerable<Err>>> Handle(
        ParentsTreeDataQueryRequest request, CancellationToken cancellationToken)
    {

        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var result = await rightsCollector.ParentsTreeData(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.ViewStyle, cancellationToken);

        return result.Match<OneOf<List<DataTypeModel>, IEnumerable<Err>>>(r => r, e => e);
    }
}