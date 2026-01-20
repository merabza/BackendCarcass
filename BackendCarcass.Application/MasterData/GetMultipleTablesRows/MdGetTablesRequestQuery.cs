using Microsoft.Extensions.Primitives;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.MasterData.GetMultipleTablesRows;

public sealed class MdGetTablesRequestQuery : IQuery<MdGetTablesQueryResponse>
{
    //public MdGetTablesQueryRequest(HttpRequest httpRequest)
    //{
    //    HttpRequest = httpRequest;
    //}

    //public HttpRequest HttpRequest { get; set; } //+

    // ReSharper disable once ConvertToPrimaryConstructor
    public MdGetTablesRequestQuery(StringValues tables)
    {
        Tables = tables;
    }

    public StringValues Tables { get; init; } //+
}