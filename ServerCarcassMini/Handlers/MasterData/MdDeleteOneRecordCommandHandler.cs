using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom;
using MediatR;
using MessagingAbstractions;
using OneOf;
using ServerCarcassMini.CommandRequests.MasterData;
using SystemToolsShared;

namespace ServerCarcassMini.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdDeleteOneRecordCommandHandler : ICommandHandler<MdDeleteOneRecordCommandRequest>
{
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;

    public MdDeleteOneRecordCommandHandler(IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(MdDeleteOneRecordCommandRequest request,
        CancellationToken cancellationToken)
    {
        var masterDataCruder = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        var result = await masterDataCruder.Delete(request.Id);
        return result.Match<OneOf<Unit, IEnumerable<Err>>>(
            y =>
            {
                var errors = y.ToList();
                errors.Add(MasterDataApiErrors.CannotDeleteNewRecord);
                return errors;
            }, () => new Unit());
    }
}