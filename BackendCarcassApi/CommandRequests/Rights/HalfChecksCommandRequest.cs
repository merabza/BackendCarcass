using System.Collections.Generic;
using CarcassRepositories;
using CarcassRepositories.Models;
using MessagingAbstractions;
using Microsoft.AspNetCore.Http;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class HalfChecksCommandRequest : ICommand<List<TypeDataModel>>
{
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