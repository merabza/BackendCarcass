using System.Collections.Generic;
using CarcassDb.QueryModels;
using CarcassRepositories;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.QueryRequests.Rights;

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