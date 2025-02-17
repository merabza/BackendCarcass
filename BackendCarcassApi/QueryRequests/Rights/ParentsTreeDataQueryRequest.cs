using System.Collections.Generic;
using CarcassDom;
using CarcassDom.Models;
using MessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.Rights;

public sealed class ParentsTreeDataQueryRequest : IQuery<List<DataTypeModel>>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ParentsTreeDataQueryRequest(ERightsEditorViewStyle viewStyle)
    {
        ViewStyle = viewStyle;
    }


    public ERightsEditorViewStyle ViewStyle { get; }
}