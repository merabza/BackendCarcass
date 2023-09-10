using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.MasterData;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    MdCreateOneRecordCommandHandler : ICommandHandler<MdCreateOneRecordCommandRequest, MasterDataCrudLoadedData>
{
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;


    public MdCreateOneRecordCommandHandler(IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<MasterDataCrudLoadedData, IEnumerable<Err>>> Handle(
        MdCreateOneRecordCommandRequest request, CancellationToken cancellationToken)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        using StreamReader reader = new(request.HttpRequest.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var masterDataCruder = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        var crudData = new MasterDataCrudData(body);
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