using System.Collections.Generic;
using System.Linq;
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

namespace BackendCarcass.Application.Rights.GetParentsTree;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ParentsTreeDataQueryHandler(
    IRightsRepository repo,
    IReturnValuesRepository rvRepo,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : IQueryHandlerOmd<ParentsTreeDataRequestQuery, List<DataTypeModel>>
{
    public async Task<OneOf<List<DataTypeModel>, ErrorOmd[]>> Handle(ParentsTreeDataRequestQuery request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(repo, rvRepo, databaseAbstraction);
        OneOf<List<DataTypeModel>, ErrorOmd[]> result =
            await rightsCollector.ParentsTreeData(currentUser.Name, request.ViewStyle, cancellationToken);

        return result.Match<OneOf<List<DataTypeModel>, ErrorOmd[]>>(r => r, e => e.ToArray());
    }
}
