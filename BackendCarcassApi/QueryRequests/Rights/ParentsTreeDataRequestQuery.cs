using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MediatRMessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.Rights;

public sealed class ParentsTreeDataRequestQuery : IQuery<List<DataTypeModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ParentsTreeDataRequestQuery(ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
    }

    public ERightsEditorViewStyle ViewStyle { get; }
}