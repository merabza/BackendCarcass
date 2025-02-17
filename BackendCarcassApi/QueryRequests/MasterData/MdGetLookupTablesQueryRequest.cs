﻿using BackendCarcassApi.QueryResponses;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed class MdGetLookupTablesQueryRequest(HttpRequest httpRequest) : IQuery<MdGetLookupTablesQueryResponse>
{
    public HttpRequest HttpRequest { get; init; } = httpRequest;//+
}