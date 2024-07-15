using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.Rights;
using CarcassDom;
using CarcassDom.Models;
using CarcassMasterDataDom;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChildrenTreeDataQueryHandler : ICommandHandler<ChildrenTreeDataCommandRequest, List<DataTypeModel>>
{
    //private readonly IMenuRightsRepository _mdRepo;
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChildrenTreeDataQueryHandler(IRightsRepository repo, IReturnValuesRepository rvRepo)
    {
        //_mdRepo = mdRepo;
        _repo = repo;
        _rvRepo = rvRepo;
    }

    public async Task<OneOf<List<DataTypeModel>, IEnumerable<Err>>> Handle(
        ChildrenTreeDataCommandRequest request, CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var result = await rightsCollector.ChildrenTreeData(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.DataTypeKey, request.ViewStyle, cancellationToken);
        return result.Match<OneOf<List<DataTypeModel>, IEnumerable<Err>>>(r => r, e => e);

        //var dataTypeModels = await _mdRepo.ChildrenTreeData(request.HttpRequest.HttpContext.User.Identity!.Name!,
        //    request.dataTypeKey, request.ViewStyle, cancellationToken);
        //return dataTypeModels;
    }
}