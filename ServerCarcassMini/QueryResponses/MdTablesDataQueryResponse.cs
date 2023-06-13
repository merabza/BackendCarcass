using System.Collections.Generic;

namespace ServerCarcassMini.QueryResponses;

public sealed class MdTablesDataQueryResponse
{
    public MdTablesDataQueryResponse(Dictionary<string, IEnumerable<dynamic>> entities)
    {
        Entities = entities;
    }

    public Dictionary<string, IEnumerable<dynamic>> Entities { get; set; }
}