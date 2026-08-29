using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Crud;
using BackendCarcass.Application.Crud.Models;
using BackendCarcass.Application.FilterSort.Models;
using BackendCarcassShared.Contracts.Errors;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

//using CrudBase = BackendCarcass.Application.Crud.CrudBase;
//using FilterSortRequest = BackendCarcass.Application.Crud.Models.FilterSortRequest;
//using FilterSortRequestFactory = BackendCarcass.Application.FilterSort.Models.FilterSortRequestFactory;
//using TableRowsData = BackendCarcass.Application.Crud.Models.TableRowsData;

namespace BackendCarcass.Application.MasterData.GetTableRows;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTableRowsDataQueryHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    : IQueryHandler<GetTableRowsDataRequestQuery, TableRowsData>
{
    public async Task<Result<TableRowsData>> Handle(GetTableRowsDataRequestQuery request,
        CancellationToken cancellationToken)
    {
        FilterSortRequest? filterSortRequestObject = FilterSortRequestFactory.Create(request.FilterSortRequest);

        if (filterSortRequestObject == null)
        {
            return Result.Failure<TableRowsData>(CommonErrors.IncorrectData);
        }

        //var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(request.tableName);
        //var result = await loader.GetTableRowsData(filterSortRequestObject, cancellationToken);
        //return result.Match<OneOf<TableRowsData, Err[]>>(
        //    r => r, e => e);

        Result<CrudBase> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsFailure)
        {
            return Result.Failure<TableRowsData>(createMasterDataCrudResult.Error);
        }

        CrudBase masterDataCruder = createMasterDataCrudResult.Value;

        return await masterDataCruder.GetTableRowsData(filterSortRequestObject, cancellationToken);
    }
}
