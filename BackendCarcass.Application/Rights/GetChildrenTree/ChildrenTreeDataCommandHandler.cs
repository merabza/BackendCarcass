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

namespace BackendCarcass.Application.Rights.GetChildrenTree;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    ChildrenTreeDataCommandHandler : ICommandHandler<ChildrenTreeDataRequestCommand, List<DataTypeModel>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChildrenTreeDataCommandHandler(IRightsRepository repo, IReturnValuesRepository rvRepo,
        IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<OneOf<List<DataTypeModel>, Err[]>> Handle(ChildrenTreeDataRequestCommand request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo, _unitOfWork);
        var result = await rightsCollector.ChildrenTreeData(_currentUser.Name, request.DataTypeKey, request.ViewStyle,
            cancellationToken);
        return result.Match<OneOf<List<DataTypeModel>, Err[]>>(r => r, e => e);
    }
}