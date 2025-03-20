using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.MasterData;
using BackendCarcassContracts.Errors;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    MdCreateOneRecordCommandHandler : ICommandHandler<MdCreateOneRecordCommandRequest, MasterDataCrudLoadedData>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    public MdCreateOneRecordCommandHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<MasterDataCrudLoadedData, IEnumerable<Err>>> Handle(MdCreateOneRecordCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        // ReSharper disable once using
        using StreamReader reader = new(request.HttpRequest.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var crudData = new MasterDataCrudData(body);
        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return (Err[])createMasterDataCrudResult.AsT1;
        var masterDataCruder = createMasterDataCrudResult.AsT0;
        var result = await masterDataCruder.Create(crudData, cancellationToken);
        return result.Match<OneOf<MasterDataCrudLoadedData, IEnumerable<Err>>>(rcd => (MasterDataCrudLoadedData)rcd,
            y =>
            {
                var errors = y.ToList();
                errors.Add(MasterDataApiErrors.CannotCreateNewRecord);
                return errors;
            });
    }
}