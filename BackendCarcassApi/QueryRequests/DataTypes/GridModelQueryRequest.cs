using MessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.DataTypes;

public sealed class GridModelQueryRequest : IQuery<string>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GridModelQueryRequest(string gridName)
    {
        GridName = gridName;
    }

    public string GridName { get; set; }
}