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
public sealed class HalfChecksQueryHandler : ICommandHandler<HalfChecksCommandRequest, List<TypeDataModel>>
{
    private readonly ICurrentUser _currentUser;

    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HalfChecksQueryHandler(IRightsRepository repo, IReturnValuesRepository rvRepo, ICurrentUser currentUser)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
    }

    public async Task<OneOf<List<TypeDataModel>, Err[]>> Handle(HalfChecksCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var typeDataModels = await rightsCollector.HalfChecks(_currentUser.Name, request.DataTypeId, request.DataKey,
            request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}