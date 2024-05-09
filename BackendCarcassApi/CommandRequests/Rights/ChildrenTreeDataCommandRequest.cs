using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class ChildrenTreeDataCommandRequest : ICommand<List<DataTypeModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ChildrenTreeDataCommandRequest(HttpRequest httpRequest, string dataTypeKey, ERightsEditorViewStyle viewStyle)
    {
        HttpRequest = httpRequest;
        ViewStyle = viewStyle;
        DataTypeKey = dataTypeKey;
    }

    public HttpRequest HttpRequest { get; set; }

    public ERightsEditorViewStyle ViewStyle { get; set; }
    public string DataTypeKey { get; set; }
}