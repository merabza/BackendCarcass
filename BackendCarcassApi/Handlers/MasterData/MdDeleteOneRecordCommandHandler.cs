using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.MasterData;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom;
using MediatR;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdDeleteOneRecordCommandHandler : ICommandHandler<MdDeleteOneRecordCommandRequest>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdDeleteOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(MdDeleteOneRecordCommandRequest request,
        CancellationToken cancellationToken)
    {
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return createMasterDataCrudResult.AsT1;
        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.Match<OneOf<Unit, IEnumerable<Err>>>(
            y =>
            {
                var errors = y.ToList();
                errors.Add(MasterDataApiErrors.CannotDeleteNewRecord);
                return errors;
            }, () => new Unit());
    }
}