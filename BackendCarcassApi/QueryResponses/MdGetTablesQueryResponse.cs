using System.Collections.Generic;

namespace BackendCarcassApi.QueryResponses;

public sealed class MdGetTablesQueryResponse
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetTablesQueryResponse(Dictionary<string, IEnumerable<dynamic>> entities)
    {
        Entities = entities;
    }

    public Dictionary<string, IEnumerable<dynamic>> Entities { get; set; }
}