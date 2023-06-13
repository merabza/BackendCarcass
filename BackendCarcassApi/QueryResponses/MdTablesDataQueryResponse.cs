using System.Collections.Generic;

namespace BackendCarcassApi.QueryResponses;

public sealed class MdTablesDataQueryResponse
{
    public MdTablesDataQueryResponse(Dictionary<string, IEnumerable<dynamic>> entities)
    {
        Entities = entities;
    }

    public Dictionary<string, IEnumerable<dynamic>> Entities { get; set; }
}