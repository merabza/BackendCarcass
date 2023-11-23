using System.Collections.Generic;
using CarcassMasterDataDom.Models;

namespace BackendCarcassApi.QueryResponses;

public sealed class MdGetLookupTablesQueryResponse(Dictionary<string, IEnumerable<ReturnValueModel>> returnValues)
{
    public Dictionary<string, IEnumerable<ReturnValueModel>> ReturnValues { get; set; } = returnValues;
}