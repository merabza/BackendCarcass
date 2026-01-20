using System.Collections.Generic;
using BackendCarcass.Rights;
using BackendCarcass.Rights.Models;
using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.Rights.GetParentsTree;

public sealed class ParentsTreeDataRequestQuery : IQuery<List<DataTypeModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ParentsTreeDataRequestQuery(ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
    }

    public ERightsEditorViewStyle ViewStyle { get; }
}