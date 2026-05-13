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

namespace BackendCarcass.Application.Rights.GetHalfChecks;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HalfChecksCommandHandler(
    IRightsRepository repo,
    IReturnValuesRepository rvRepo,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : ICommandHandler<HalfChecksRequestCommand, List<TypeDataModel>>
{
    public async Task<OneOf<List<TypeDataModel>, Error[]>> Handle(HalfChecksRequestCommand request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(repo, rvRepo, databaseAbstraction);
        List<TypeDataModel> typeDataModels = await rightsCollector.HalfChecks(currentUser.Name, request.DataTypeId,
            request.DataKey, request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}
