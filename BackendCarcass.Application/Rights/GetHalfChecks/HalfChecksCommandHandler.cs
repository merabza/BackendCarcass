using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using OneOf;
using SystemTools.Domain.Abstractions;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Rights.GetHalfChecks;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HalfChecksCommandHandler : ICommandHandler<HalfChecksRequestCommand, List<TypeDataModel>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IDatabaseAbstraction _databaseAbstraction;

    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HalfChecksCommandHandler(IRightsRepository repo, IReturnValuesRepository rvRepo, IUnitOfWork unitOfWork,
        ICurrentUser currentUser, IDatabaseAbstraction databaseAbstraction)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
        _databaseAbstraction = databaseAbstraction;
        _unitOfWork = unitOfWork;
    }

    public async Task<OneOf<List<TypeDataModel>, Error[]>> Handle(HalfChecksRequestCommand request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo, _unitOfWork, _databaseAbstraction);
        List<TypeDataModel> typeDataModels = await rightsCollector.HalfChecks(_currentUser.Name, request.DataTypeId,
            request.DataKey, request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}
