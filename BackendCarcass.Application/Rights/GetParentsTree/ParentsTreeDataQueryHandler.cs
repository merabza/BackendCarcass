using System.Collections.Generic;
using System.Linq;
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

namespace BackendCarcass.Application.Rights.GetParentsTree;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ParentsTreeDataQueryHandler : IQueryHandler<ParentsTreeDataRequestQuery, List<DataTypeModel>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IRightsRepository _repo;
    private readonly IReturnValuesRepository _rvRepo;
    private readonly IUnitOfWork _unitOfWork;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ParentsTreeDataQueryHandler(IRightsRepository repo, IReturnValuesRepository rvRepo, IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repo = repo;
        _rvRepo = rvRepo;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<OneOf<List<DataTypeModel>, Err[]>> Handle(ParentsTreeDataRequestQuery request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(_repo, _rvRepo, _unitOfWork);
        var result = await rightsCollector.ParentsTreeData(_currentUser.Name, request.ViewStyle, cancellationToken);

        return result.Match<OneOf<List<DataTypeModel>, Err[]>>(r => r, e => e.ToArray());
    }
}