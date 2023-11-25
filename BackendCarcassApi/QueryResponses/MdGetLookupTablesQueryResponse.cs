using System.Collections.Generic;
using CarcassMasterDataDom.Models;

namespace BackendCarcassApi.QueryResponses;

public sealed class MdGetLookupTablesQueryResponse
{
    public MdGetLookupTablesQueryResponse(Dictionary<string, IEnumerable<ReturnValueModel>> returnValues)
    {
        ReturnValues = returnValues;
    }

    public Dictionary<string, IEnumerable<ReturnValueModel>> ReturnValues { get; set; }
}