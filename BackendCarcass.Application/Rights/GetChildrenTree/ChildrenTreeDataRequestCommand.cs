using System.Collections.Generic;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.Rights.GetChildrenTree;

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
