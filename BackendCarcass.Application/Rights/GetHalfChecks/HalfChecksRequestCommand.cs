using System.Collections.Generic;
using BackendCarcass.Application.Rights.Models;
using SystemTools.Application.Abstractions.Messaging;

//using TypeDataModel = BackendCarcass.Application.Rights.Models.TypeDataModel;

namespace BackendCarcass.Application.Rights.GetHalfChecks;

public sealed class HalfChecksRequestCommand : ICommand<List<TypeDataModel>>
{
    public HalfChecksRequestCommand(int dataTypeId, string dataKey, ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
        DataTypeId = dataTypeId;
        DataKey = dataKey;
    }

    public ERightsEditorViewStyle ViewStyle { get; }
    public int DataTypeId { get; }
    public string DataKey { get; }
}
