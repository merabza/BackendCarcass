using Microsoft.Extensions.Primitives;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.MasterData.GetLookupTables;

public sealed class MdGetLookupTablesRequestQuery : IQuery<MdGetLookupTablesQueryResponse>
{
    //StringValues tables
    //public MdGetLookupTablesQueryRequest(HttpRequest httpRequest)
    //{
    //    HttpRequest = httpRequest;
    //}

    //public HttpRequest HttpRequest { get; init; } //+

    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetLookupTablesRequestQuery(StringValues tables)
    {
        Tables = tables;
    }

    public StringValues Tables { get; init; } //+
}
