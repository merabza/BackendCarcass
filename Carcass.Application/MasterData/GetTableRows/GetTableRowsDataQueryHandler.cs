using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassContracts.Errors;
using CarcassFilterSort.Models;
using CarcassMasterData;
using LibCrud.Models;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace Carcass.Application.MasterData.GetTableRows;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTableRowsDataQueryHandler : IQueryHandler<GetTableRowsDataRequestQuery, TableRowsData>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCrudCreator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetTableRowsDataQueryHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<TableRowsData, Err[]>> Handle(GetTableRowsDataRequestQuery request,
        CancellationToken cancellationToken)
    {
        var filterSortRequestObject = FilterSortRequestFactory.Create(request.FilterSortRequest);

        if (filterSortRequestObject == null)
        {
            return new[] { CommonErrors.IncorrectData };
        }

        //var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(request.tableName);
        //var result = await loader.GetTableRowsData(filterSortRequestObject, cancellationToken);
        //return result.Match<OneOf<TableRowsData, Err[]>>(
        //    r => r, e => e);

        var createMasterDataCrudResult = _masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        var masterDataCruder = createMasterDataCrudResult.AsT0;

        var result = await masterDataCruder.GetTableRowsData(filterSortRequestObject, cancellationToken);
        return result.Match<OneOf<TableRowsData, Err[]>>(r => r, e => e.ToArray());
    }
}
