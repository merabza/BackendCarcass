﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.MasterData;
using BackendCarcassApi.QueryResponses;
using CarcassMasterDataDom;
using MediatRMessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GetTablesQueryHandler : IQueryHandler<MdGetTablesQueryRequest, MdGetTablesQueryResponse>
{
    private readonly IMasterDataLoaderCreator _masterDataLoaderCreator;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GetTablesQueryHandler(IMasterDataLoaderCreator masterDataLoaderCreator)
    {
        _masterDataLoaderCreator = masterDataLoaderCreator;
    }

    public async Task<OneOf<MdGetTablesQueryResponse, IEnumerable<Err>>> Handle(MdGetTablesQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        List<string> tableNames = request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()
            .ToList()!;
        var mdLoader = new MasterDataLoader(tableNames, _masterDataLoaderCreator);
        var loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetTablesQueryResponse, IEnumerable<Err>>>(
            r => new MdGetTablesQueryResponse(r), e => (Err[])e);
    }
}