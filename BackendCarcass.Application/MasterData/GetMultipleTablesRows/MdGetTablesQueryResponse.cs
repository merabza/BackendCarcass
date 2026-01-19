using System.Collections.Generic;

namespace Carcass.Application.MasterData.GetMultipleTablesRows;

public sealed class MdGetTablesQueryResponse
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetTablesQueryResponse(Dictionary<string, IEnumerable<dynamic>> entities)
    {
        Entities = entities;
    }

    public Dictionary<string, IEnumerable<dynamic>> Entities { get; set; }
}