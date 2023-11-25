using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.QueryRequests.Rights;

public sealed class ParentsTreeDataQueryRequest : IQuery<List<DataTypeModel>>
{
    public ParentsTreeDataQueryRequest(HttpRequest httpRequest, ERightsEditorViewStyle viewStyle)
    {
        HttpRequest = httpRequest;
        ViewStyle = viewStyle;
    }

    public HttpRequest HttpRequest { get; set; }

    public ERightsEditorViewStyle ViewStyle { get; }
}