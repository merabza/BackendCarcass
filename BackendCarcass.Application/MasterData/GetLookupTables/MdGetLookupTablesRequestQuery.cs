using Microsoft.Extensions.Primitives;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.MasterData.GetLookupTables;

public sealed class MdGetLookupTablesRequestQuery : IQuery<MdGetLookupTablesQueryResponse>
{
    //StringValues tables
    //public MdGetLookupTablesQueryRequest(HttpRequest httpRequest)
    //{
    //    HttpRequest = httpRequest;
    //}

    //public HttpRequest HttpRequest { get; init; } //+

    public MdGetLookupTablesRequestQuery(StringValues tables)
    {
        Tables = tables;
    }

    public StringValues Tables { get; init; } //+
}
