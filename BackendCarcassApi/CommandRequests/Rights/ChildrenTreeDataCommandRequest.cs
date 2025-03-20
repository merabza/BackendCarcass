using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class ChildrenTreeDataCommandRequest : ICommand<List<DataTypeModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ChildrenTreeDataCommandRequest(string dataTypeKey, ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
        DataTypeKey = dataTypeKey;
    }

    public ERightsEditorViewStyle ViewStyle { get; set; }
    public string DataTypeKey { get; set; }
}