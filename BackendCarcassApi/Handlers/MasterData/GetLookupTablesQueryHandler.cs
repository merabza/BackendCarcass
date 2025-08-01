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

// ReSharper disable ConvertToPrimaryConstructor

namespace BackendCarcassApi.Handlers.MasterData;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class
    GetLookupTablesQueryHandler : IQueryHandler<MdGetLookupTablesQueryRequest, MdGetLookupTablesQueryResponse>
{
    private readonly IReturnValuesLoaderCreator _returnValuesLoaderCreator;
    private readonly IReturnValuesRepository _rvRepo;

    public GetLookupTablesQueryHandler(IReturnValuesRepository rvRepo,
        IReturnValuesLoaderCreator returnValuesLoaderCreator)
    {
        _rvRepo = rvRepo;
        _returnValuesLoaderCreator = returnValuesLoaderCreator;
    }

    public async Task<OneOf<MdGetLookupTablesQueryResponse, IEnumerable<Err>>> Handle(
        MdGetLookupTablesQueryRequest request, CancellationToken cancellationToken = default)
    {
        //var reqQuery = request.HttpRequest.Query["tables"];
        List<string> tableNames = request.Tables.Where(tableName => !string.IsNullOrWhiteSpace(tableName)).Distinct()
            .ToList()!;
        var mdLoader = new ReturnValuesLoader(tableNames, _rvRepo, _returnValuesLoaderCreator);
        var loaderResult = await mdLoader.Run(cancellationToken);
        return loaderResult.Match<OneOf<MdGetLookupTablesQueryResponse, IEnumerable<Err>>>(
            r => new MdGetLookupTablesQueryResponse(r), e => e.ToArray());
    }
}