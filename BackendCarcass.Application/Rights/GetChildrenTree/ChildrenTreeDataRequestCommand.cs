using System.Collections.Generic;
using BackendCarcass.Application.Rights.Models;
using SystemTools.Application.Abstractions.Messaging;

//using DataTypeModel = BackendCarcass.Application.Rights.Models.DataTypeModel;

namespace BackendCarcass.Application.Rights.GetChildrenTree;

public sealed class ChildrenTreeDataRequestCommand : ICommand<List<DataTypeModel>>
{
    public ChildrenTreeDataRequestCommand(string dataTypeKey, ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
        DataTypeKey = dataTypeKey;
    }

    public ERightsEditorViewStyle ViewStyle { get; set; }
    public string DataTypeKey { get; set; }
}
