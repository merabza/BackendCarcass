using System.Collections.Generic;

namespace ServerCarcassMini.QueryResponses;

public sealed class MdGetTableAllRecordsQueryResponse
{
    public MdGetTableAllRecordsQueryResponse(IEnumerable<dynamic> entities)
    {
        Entities = entities;
    }

    public IEnumerable<dynamic> Entities { get; set; }
}