using System.Collections.Generic;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.Rights.GetHalfChecks;

public sealed class HalfChecksRequestCommand : ICommand<List<TypeDataModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
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
