using System.Collections.Generic;
using CarcassRights;
using CarcassRights.Models;
using MediatRMessagingAbstractions;

namespace Carcass.Application.Rights.GetChildrenTree;

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