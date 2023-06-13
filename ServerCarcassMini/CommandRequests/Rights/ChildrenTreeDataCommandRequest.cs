using System.Collections.Generic;
using CarcassDb.QueryModels;
using CarcassRepositories;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace ServerCarcassMini.CommandRequests.Rights;

public sealed class ChildrenTreeDataCommandRequest : ICommand<List<DataTypeModel>>
{
    public ChildrenTreeDataCommandRequest(HttpRequest httpRequest, string dataTypeKey, ERightsEditorViewStyle viewStyle)
    {
        HttpRequest = httpRequest;
        ViewStyle = viewStyle;
        this.dataTypeKey = dataTypeKey;
    }

    public HttpRequest HttpRequest { get; set; }

    public ERightsEditorViewStyle ViewStyle { get; set; }
    public string dataTypeKey { get; set; }
}