using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData;
using BackendCarcass.Application.Rights.Models;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Application.Rights.GetParentsTree;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ParentsTreeDataQueryHandler(
    IRightsRepository repo,
    IReturnValuesRepository rvRepo,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : IQueryHandler<ParentsTreeDataRequestQuery, List<DataTypeModel>>
{
    public async Task<Result<List<DataTypeModel>>> Handle(ParentsTreeDataRequestQuery request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(repo, rvRepo, databaseAbstraction);
        return await rightsCollector.ParentsTreeData(currentUser.Name, request.ViewStyle, cancellationToken);
    }
}
