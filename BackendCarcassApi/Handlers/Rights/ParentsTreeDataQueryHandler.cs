﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.QueryRequests.Rights;
using CarcassDb.QueryModels;
using CarcassRepositories;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared;

namespace BackendCarcassApi.Handlers.Rights;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class ParentsTreeDataQueryHandler : IQueryHandler<ParentsTreeDataQueryRequest, List<DataTypeModel>>
{
    private readonly IMenuRightsRepository _mdRepo;

    public ParentsTreeDataQueryHandler(IMenuRightsRepository mdRepo)
    {
        _mdRepo = mdRepo;
    }

    public async Task<OneOf<List<DataTypeModel>, IEnumerable<Err>>> Handle(
        ParentsTreeDataQueryRequest request, CancellationToken cancellationToken)
    {
        var dataTypeModels = await _mdRepo.ParentsTreeData(request.HttpRequest.HttpContext.User.Identity!.Name!,
            request.ViewStyle, cancellationToken);
        return dataTypeModels;
    }
}