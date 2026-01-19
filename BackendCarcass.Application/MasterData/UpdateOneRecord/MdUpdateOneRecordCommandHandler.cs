using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassMasterData;
using CarcassMasterData.Models;
using MediatR;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace Carcass.Application.MasterData.UpdateOneRecord;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdUpdateOneRecordCommandHandler : ICommandHandler<MdUpdateOneRecordRequestCommand>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdUpdateOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(MdUpdateOneRecordRequestCommand request,
        CancellationToken cancellationToken)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        // ReSharper disable once using
        // ReSharper disable once DisposableConstructor
        using var reader = new StreamReader(request.HttpRequest.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var crudData = new MasterDataCrudData(body);
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.Update(request.Id, crudData, cancellationToken);
        return result.Match<OneOf<Unit, Err[]>>(y =>
        {
            var errors = y.ToList();
            errors.Add(MasterDataApiErrors.CannotUpdateNewRecord);
            return errors.ToArray();
        }, () => new Unit());
    }
}
