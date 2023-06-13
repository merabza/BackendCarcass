using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarcassContracts.ErrorModels;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MediatR;
using MessagingAbstractions;
using OneOf;
using ServerCarcassMini.CommandRequests.MasterData;
using SystemToolsShared;

namespace ServerCarcassMini.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MdUpdateOneRecordCommandHandler : ICommandHandler<MdUpdateOneRecordCommandRequest>
{
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;

    public MdUpdateOneRecordCommandHandler(IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(MdUpdateOneRecordCommandRequest request,
        CancellationToken cancellationToken)
    {
        //ამოვიღოთ მოთხოვნის ტანი
        using StreamReader reader = new(request.HttpRequest.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var masterDataCruder = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        var crudData = new MasterDataCrudData(body);
        var result = await masterDataCruder.Update(request.Id, crudData);
        return result.Match<OneOf<Unit, IEnumerable<Err>>>(
            y =>
            {
                var errors = y.ToList();
                errors.Add(MasterDataApiErrors.CannotUpdateNewRecord);
                return errors;
            }, () => new Unit());
    }
}