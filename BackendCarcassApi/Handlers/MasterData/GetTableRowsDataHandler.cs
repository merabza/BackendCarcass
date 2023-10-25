﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.MasterData;
using CarcassContracts.ErrorModels;
using CarcassDom.Models;
using CarcassMasterDataDom;
using CarcassMasterDataDom.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTableRowsDataHandler : IQueryHandler<GetTableRowsDataQueryRequest, TableRowsData>
{
    private readonly IMasterDataLoaderCrudCreator _masterDataLoaderCrudCreator;

    public GetTableRowsDataHandler(IMasterDataLoaderCrudCreator masterDataLoaderCrudCreator)
    {
        _masterDataLoaderCrudCreator = masterDataLoaderCrudCreator;
    }

    public async Task<OneOf<TableRowsData, IEnumerable<Err>>> Handle(GetTableRowsDataQueryRequest request,
        CancellationToken cancellationToken)
    {
        var filterSortRequestObject = FilterSortRequestFabric.Create(request.FilterSortRequest);

        if (filterSortRequestObject == null)
            return new[] { CommonErrors.IncorrectData };

        var loader = _masterDataLoaderCrudCreator.CreateMasterDataLoader(request.tableName);
        var result = await loader.GetTableRowsData(filterSortRequestObject, cancellationToken);
        return result.Match<OneOf<TableRowsData, IEnumerable<Err>>>(
            r => r, e => e);



    }

}