using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class HalfChecksCommandRequest : ICommand<List<TypeDataModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public HalfChecksCommandRequest(HttpRequest httpRequest, int dataTypeId, string dataKey,
        ERightsEditorViewStyle viewStyle)
    {
        HttpRequest = httpRequest;
        ViewStyle = viewStyle;
        DataTypeId = dataTypeId;
        DataKey = dataKey;
    }

    public HttpRequest HttpRequest { get; set; }
    public ERightsEditorViewStyle ViewStyle { get; }
    public int DataTypeId { get; }
    public string DataKey { get; }
}