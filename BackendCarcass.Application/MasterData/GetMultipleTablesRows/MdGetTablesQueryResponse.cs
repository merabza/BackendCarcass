using System.Collections.Generic;

namespace BackendCarcass.Application.MasterData.GetMultipleTablesRows;

public sealed class MdGetTablesQueryResponse
{
    public MdGetTablesQueryResponse(Dictionary<string, IEnumerable<dynamic>> entities)
    {
        Entities = entities;
    }

    public Dictionary<string, IEnumerable<dynamic>> Entities { get; set; }
}
