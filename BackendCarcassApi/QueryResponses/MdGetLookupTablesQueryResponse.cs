using System.Collections.Generic;
using CarcassMasterDataDom.Models;

namespace BackendCarcassApi.QueryResponses;

public sealed record MdGetLookupTablesQueryResponse(Dictionary<string, IEnumerable<SrvModel>> Srv);