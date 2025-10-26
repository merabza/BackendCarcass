using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.MasterData;
using BackendCarcassContracts.Errors;
using CarcassDom.Models;
using CarcassMasterDataDom;
using LibCrud.Models;
using MediatRMessagingAbstractions;
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

    public async Task<OneOf<TableRowsData, Err[]>> Handle(GetTableRowsDataQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var filterSortRequestObject = FilterSortRequestFactory.Create(request.FilterSortRequest);

        if (filterSortRequestObject == null)
            return new[] { CommonErrors.IncorrectData };

        //var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(request.tableName);
        //var result = await loader.GetTableRowsData(filterSortRequestObject, cancellationToken);
        //return result.Match<OneOf<TableRowsData, Err[]>>(
        //    r => r, e => e);

        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
            return createMasterDataCrudResult.AsT1;
        var masterDataCruder = createMasterDataCrudResult.AsT0;

        var result = await masterDataCruder.GetTableRowsData(filterSortRequestObject, cancellationToken);
        return result.Match<OneOf<TableRowsData, Err[]>>(r => r, e => e.ToArray());
    }
}