using SystemTools.MediatRMessagingAbstractions;

namespace BackendCarcass.Application.DataTypes.GetGridModel;

public sealed class GridModelRequestQuery : IQueryOmd<string>
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public GridModelRequestQuery(string gridName)
    {
        GridName = gridName;
    }

    public string GridName { get; set; }
}
