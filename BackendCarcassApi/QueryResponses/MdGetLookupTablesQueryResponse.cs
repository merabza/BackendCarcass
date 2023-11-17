using System.Collections.Generic;

namespace BackendCarcassApi.QueryResponses;

public sealed class MdGetLookupTablesQueryResponse
{
    public MdGetLookupTablesQueryResponse(Dictionary<string, IEnumerable<dynamic>> entities)
    {
        Entities = entities;
    }

    public Dictionary<string, IEnumerable<dynamic>> Entities { get; set; }
}