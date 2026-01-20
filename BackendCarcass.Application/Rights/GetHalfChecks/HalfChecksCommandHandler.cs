using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using OneOf;
using SystemTools.DomainShared.Repositories;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Rights.GetHalfChecks;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HalfChecksCommandHandler : ICommandHandler<HalfChecksRequestCommand, List<TypeDataModel>>
{
    private readonly ICurrentUser _currentUser;

    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HalfChecksCommandHandler(IRightsRepository repo, IReturnValuesRepository rvRepo, IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<OneOf<List<TypeDataModel>, Err[]>> Handle(HalfChecksRequestCommand request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo, _unitOfWork);
        var typeDataModels = await rightsCollector.HalfChecks(_currentUser.Name, request.DataTypeId, request.DataKey,
            request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}