using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.CommandRequests.Rights;

public sealed class ChildrenTreeDataRequestCommand : ICommand<List<DataTypeModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ChildrenTreeDataRequestCommand(string dataTypeKey, ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
        DataTypeKey = dataTypeKey;
    }

    public ERightsEditorViewStyle ViewStyle { get; set; }
    public string DataTypeKey { get; set; }
}