using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.MasterData;
using BackendCarcassContracts.Errors;
using CarcassDom.Models;
using CarcassMasterDataDom;
using LibCrud.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTableRowsDataHandler : IQueryHandler<GetTableRowsDataQueryRequest, TableRowsData>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetTableRowsDataHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<TableRowsData, IEnumerable<Err>>> Handle(GetTableRowsDataQueryRequest request,
        CancellationToken cancellationToken)
    {
        var filterSortRequestObject = FilterSortRequestFabric.Create(request.FilterSortRequest);

        if (filterSortRequestObject == null)
            return new[] { CommonErrors.IncorrectData };

        //var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(request.tableName);
        //var result = await loader.GetTableRowsData(filterSortRequestObject, cancellationToken);
        //return result.Match<OneOf<TableRowsData, IEnumerable<Err>>>(
        //    r => r, e => e);

        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return createMasterDataCrudResult.AsT1;
        var masterDataCruder = createMasterDataCrudResult.AsT0;

        var result = await masterDataCruder.GetTableRowsData(filterSortRequestObject, cancellationToken);
        return result.Match<OneOf<TableRowsData, IEnumerable<Err>>>(r => r, e => e);
    }
}