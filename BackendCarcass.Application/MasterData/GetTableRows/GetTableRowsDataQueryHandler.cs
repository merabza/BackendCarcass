using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.FilterSort.Models;
using BackendCarcass.LibCrud;
using BackendCarcass.LibCrud.Models;
using BackendCarcass.MasterData;
using BackendCarcassShared.Contracts.Errors;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

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
