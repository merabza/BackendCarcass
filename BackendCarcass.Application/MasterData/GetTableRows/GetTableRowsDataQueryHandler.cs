using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.FilterSort.Models;
using BackendCarcass.LibCrud;
using BackendCarcass.LibCrud.Models;
using BackendCarcass.MasterData;
using BackendCarcassShared.Contracts.Errors;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.MasterData.GetTableRows;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTableRowsDataQueryHandler(IMasterDataLoaderCreator masterDataLoaderCrudCreator)
    : IQueryHandler<GetTableRowsDataRequestQuery, TableRowsData>
{
    public async Task<OneOf<TableRowsData, ErrorOmd[]>> Handle(GetTableRowsDataRequestQuery request,
        CancellationToken cancellationToken)
    {
        FilterSortRequest? filterSortRequestObject = FilterSortRequestFactory.Create(request.FilterSortRequest);

        if (filterSortRequestObject == null)
        {
            return new[] { CommonErrors.IncorrectData };
        }

        //var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(request.tableName);
        //var result = await loader.GetTableRowsData(filterSortRequestObject, cancellationToken);
        //return result.Match<OneOf<TableRowsData, Err[]>>(
        //    r => r, e => e);

        OneOf<CrudBase, ErrorOmd[]> createMasterDataCrudResult =
            masterDataLoaderCrudCreator.CreateMasterDataCrud(request.TableName);
        if (createMasterDataCrudResult.IsT1)
        {
            return createMasterDataCrudResult.AsT1;
        }

        CrudBase? masterDataCruder = createMasterDataCrudResult.AsT0;

        OneOf<TableRowsData, ErrorOmd[]> result =
            await masterDataCruder.GetTableRowsData(filterSortRequestObject, cancellationToken);
        return result.Match<OneOf<TableRowsData, ErrorOmd[]>>(r => r, e => e.ToArray());
    }
}
