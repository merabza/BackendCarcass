using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassIdentity;
using CarcassMasterData;
using CarcassRights;
using CarcassRights.Models;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.Rights.GetHalfChecks;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HalfChecksCommandHandler : ICommandHandler<HalfChecksRequestCommand, List<TypeDataModel>>
{
    private readonly ICurrentUser _currentUser;

    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HalfChecksCommandHandler(IRightsRepository repo, IReturnValuesRepository rvRepo, ICurrentUser currentUser)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
    }

    public async Task<OneOf<List<TypeDataModel>, Err[]>> Handle(HalfChecksRequestCommand request,
        CancellationToken cancellationToken = default)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo);
        var typeDataModels = await rightsCollector.HalfChecks(_currentUser.Name, request.DataTypeId, request.DataKey,
            request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}