using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData;
using BackendCarcassShared.Contracts.Errors;
using LanguageExt;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;
using Unit = MediatR.Unit;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.DeleteOneRecord;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdDeleteOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    : ICommandHandler<MdDeleteOneRecordRequestCommand>
{
    public async Task<OneOf<Unit, ErrorOmd[]>> Handle(MdDeleteOneRecordRequestCommand request,
        CancellationToken cancellationToken)
    {
        OneOf<CrudBase, ErrorOmd[]> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        CrudBase? masterDataCruder = createMasterDataCrudResult.AsT0;
        Option<ErrorOmd[]> result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.Match<OneOf<Unit, ErrorOmd[]>>(y =>
        {
            List<ErrorOmd> errors =
            [
                .. y,
                MasterDataApiErrors.CannotDeleteNewRecord
            ];
            return errors.ToArray();
        }, () => new Unit());
    }
}
