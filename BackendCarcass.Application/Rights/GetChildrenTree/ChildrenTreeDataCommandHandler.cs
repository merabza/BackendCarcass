using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace BackendCarcass.Application.Rights.GetChildrenTree;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChildrenTreeDataCommandHandler(
    IRightsRepository repo,
    IReturnValuesRepository rvRepo,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : ICommandHandler<ChildrenTreeDataRequestCommand, List<DataTypeModel>>
{
    public async Task<Result<List<DataTypeModel>>> Handle(ChildrenTreeDataRequestCommand request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(repo, rvRepo, databaseAbstraction);
        return await rightsCollector.ChildrenTreeData(currentUser.Name, request.DataTypeKey, request.ViewStyle,
            cancellationToken);
    }
}
