using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.LibCrud;
using BackendCarcass.MasterData;
using BackendCarcass.MasterData.Models;
using BackendCarcassShared.BackendCarcassContracts.Errors;
using LanguageExt;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;
using Unit = MediatR.Unit;

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcass.Application.MasterData.UpdateOneRecord;

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
        string body = await reader.ReadToEndAsync(cancellationToken);

        var crudData = new MasterDataCrudData(body);
        OneOf<CrudBase, Err[]> createMasterDataCrudResult =
            _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        CrudBase? masterDataCruder = createMasterDataCrudResult.AsT0;
        Option<Err[]> result = await masterDataCruder.Update(request.Id, crudData, cancellationToken);
        return result.Match<OneOf<Unit, Err[]>>(y =>
        {
            List<Err> errors = y.ToList();
            errors.Add(MasterDataApiErrors.CannotUpdateNewRecord);
            return errors.ToArray();
        }, () => new Unit());
    }
}
