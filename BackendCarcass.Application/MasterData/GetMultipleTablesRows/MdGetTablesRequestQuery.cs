using Microsoft.Extensions.Primitives;
using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.MasterData.GetMultipleTablesRows;

public sealed class MdGetTablesRequestQuery : IQuery<MdGetTablesQueryResponse>
{
    //public MdGetTablesQueryRequest(HttpRequest httpRequest)
    //{
    //    HttpRequest = httpRequest;
    //}

    //public HttpRequest HttpRequest { get; set; } //+

    public MdGetTablesRequestQuery(StringValues tables)
    {
        Tables = tables;
    }

    public StringValues Tables { get; init; } //+
}
