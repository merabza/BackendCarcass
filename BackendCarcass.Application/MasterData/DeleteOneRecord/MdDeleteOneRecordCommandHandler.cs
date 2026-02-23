using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData;
using BackendCarcassShared.BackendCarcassContracts.Errors;
using LanguageExt;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;
using Unit = MediatR.Unit;

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
        OneOf<CrudBase, Err[]> createMasterDataCrudResult =
            _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        CrudBase? masterDataCruder = createMasterDataCrudResult.AsT0;
        Option<Err[]> result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.Match<OneOf<Unit, Err[]>>(y =>
        {
            List<Err> errors = y.ToList();
            errors.Add(MasterDataApiErrors.CannotDeleteNewRecord);
            return errors.ToArray();
        }, () => new Unit());
    }
}
