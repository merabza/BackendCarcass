using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.Rights.GetChildrenTree;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChildrenTreeDataCommandHandler(
    IRightsRepository repo,
    IReturnValuesRepository rvRepo,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : ICommandHandlerOmd<ChildrenTreeDataRequestCommand, List<DataTypeModel>>
{
    public async Task<OneOf<List<DataTypeModel>, ErrorOmd[]>> Handle(ChildrenTreeDataRequestCommand request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(repo, rvRepo, databaseAbstraction);
        OneOf<List<DataTypeModel>, ErrorOmd[]> result = await rightsCollector.ChildrenTreeData(currentUser.Name,
            request.DataTypeKey, request.ViewStyle, cancellationToken);
        return result.Match<OneOf<List<DataTypeModel>, ErrorOmd[]>>(r => r, e => e);
    }
}
