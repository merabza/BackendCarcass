using SystemTools.Application.Abstractions.Messaging;

namespace BackendCarcass.Application.DataTypes.GetGridModel;

public sealed class GridModelRequestQuery : IQuery<string>
{
    public GridModelRequestQuery(string gridName)
    {
        GridName = gridName;
    }

    public string GridName { get; set; }
}
