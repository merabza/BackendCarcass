using MediatRMessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.DataTypes;

public sealed class GridModelRequestQuery : IQuery<string>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GridModelRequestQuery(string gridName)
    {
        GridName = gridName;
    }

    public string GridName { get; set; }
}