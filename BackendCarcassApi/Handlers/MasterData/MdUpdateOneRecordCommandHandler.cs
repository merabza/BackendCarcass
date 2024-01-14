using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.MasterData;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MediatR;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdUpdateOneRecordCommandHandler : ICommandHandler<MdUpdateOneRecordCommandRequest>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdUpdateOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(MdUpdateOneRecordCommandRequest request,
        CancellationToken cancellationToken)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        using StreamReader reader = new(request.HttpRequest.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var crudData = new MasterDataCrudData(body);
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return createMasterDataCrudResult.AsT1;
        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.Update(request.Id, crudData, cancellationToken);
        return result.Match<OneOf<Unit, IEnumerable<Err>>>(
            y =>
            {
                var errors = y.ToList();
                errors.Add(MasterDataApiErrors.CannotUpdateNewRecord);
                return errors;
            }, () => new Unit());
    }
}