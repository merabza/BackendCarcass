using BackendCarcassApi.QueryResponses;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.MasterData;

public sealed record MdGetLookupTablesQueryRequest(HttpRequest HttpRequest) : IQuery<MdGetLookupTablesQueryResponse>;