using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.MasterData;
using BackendCarcassContracts.Errors;
using MediatR;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.DeleteOneRecord;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdDeleteOneRecordCommandHandler : ICommandHandler<MdDeleteOneRecordRequestCommand>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdDeleteOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(MdDeleteOneRecordRequestCommand request,
        CancellationToken cancellationToken)
    {
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.Match<OneOf<Unit, Err[]>>(y =>
        {
            var errors = y.ToList();
            errors.Add(MasterDataApiErrors.CannotDeleteNewRecord);
            return errors.ToArray();
        }, () => new Unit());
    }
}
