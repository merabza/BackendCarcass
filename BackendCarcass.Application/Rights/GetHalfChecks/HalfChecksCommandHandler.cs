using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.MasterData;
using BackendCarcass.Application.Rights.Models;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

//using ICurrentUser = BackendCarcass.Application.Identity.ICurrentUser;
//using IReturnValuesRepository = BackendCarcass.Application.MasterData.IReturnValuesRepository;
//using TypeDataModel = BackendCarcass.Application.Rights.Models.TypeDataModel;

namespace BackendCarcass.Application.Rights.GetHalfChecks;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class HalfChecksCommandHandler(
    IRightsRepository repo,
    IReturnValuesRepository rvRepo,
    ICurrentUser currentUser,
    IDatabaseAbstraction databaseAbstraction) : ICommandHandler<HalfChecksRequestCommand, List<TypeDataModel>>
{
    public async Task<Result<List<TypeDataModel>>> Handle(HalfChecksRequestCommand request,
        CancellationToken cancellationToken)
    {
        var rightsCollector = new RightsCollector(repo, rvRepo, databaseAbstraction);
        List<TypeDataModel> typeDataModels = await rightsCollector.HalfChecks(currentUser.Name, request.DataTypeId,
            request.DataKey, request.ViewStyle, cancellationToken);
        return typeDataModels;
    }
}
