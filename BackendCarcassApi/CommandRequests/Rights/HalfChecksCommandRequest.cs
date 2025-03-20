using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class HalfChecksCommandRequest : ICommand<List<TypeDataModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public HalfChecksCommandRequest(int dataTypeId, string dataKey, ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
        DataTypeId = dataTypeId;
        DataKey = dataKey;
    }

    public ERightsEditorViewStyle ViewStyle { get; }
    public int DataTypeId { get; }
    public string DataKey { get; }
}