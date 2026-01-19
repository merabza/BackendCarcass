using System.Collections.Generic;
using CarcassRights;
using CarcassRights.Models;
using MediatRMessagingAbstractions;

namespace Carcass.Application.Rights.GetParentsTree;

public sealed class ParentsTreeDataRequestQuery : IQuery<List<DataTypeModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ParentsTreeDataRequestQuery(ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
    }

    public ERightsEditorViewStyle ViewStyle { get; }
}