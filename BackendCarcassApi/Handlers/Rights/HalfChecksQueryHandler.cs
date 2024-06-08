using BackendCarcassApi.CommandRequests.Rights;
using CarcassDom;
using CarcassDom.Models;
using CarcassMasterDataDom;
using MessagingAbstractions;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HalfChecksQueryHandler : ICommandHandler<HalfChecksCommandRequest, List<TypeDataModel>>
{
    //private readonly IMenuRightsRepository _repository;
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HalfChecksQueryHandler(IRightsRepository repo, IReturnValuesRepository rvRepo)
    {
        //_repository = repository;
        _repo = repo;
        _rvRepo = rvRepo;
    }

    public async Task<OneOf<List<TypeDataModel>, IEnumerable<Err>>> Handle(
        HalfChecksCommandRequest request, CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var typeDataModels = await rightsCollector.HalfChecks(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.DataTypeId, request.DataKey, request.ViewStyle, cancellationToken);

        //var typeDataModels = await _repository.HalfChecks(request.HttpRequest.HttpContext.User.Identity!.Name!,
        //    request.DataTypeId, request.DataKey, request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}