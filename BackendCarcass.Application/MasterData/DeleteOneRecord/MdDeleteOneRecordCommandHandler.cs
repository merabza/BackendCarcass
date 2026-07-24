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
    public async Task<OneOf<Unit, Error[]>> Handle(MdDeleteOneRecordRequestCommand request,
        CancellationToken cancellationToken)
    {
        OneOf<CrudBase, Error[]> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        CrudBase? masterDataCruder = createMasterDataCrudResult.AsT0;
        Option<Error[]> result = await masterDataCruder.Delete(request.Id, cancellationToken);
        return result.Match<OneOf<Unit, Error[]>>(y =>
        {
            List<Error> errors =
            [
                .. y,
                MasterDataApiErrors.CannotDeleteNewRecord
            ];
            return errors.ToArray();
        }, () => new Unit());
    }
}
