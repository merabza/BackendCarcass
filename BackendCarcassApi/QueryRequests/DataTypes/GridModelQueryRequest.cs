﻿using MessagingAbstractions;

namespace BackendCarcassApi.QueryRequests.DataTypes;

public sealed class GridModelQueryRequest : IQuery<string>
{
    public GridModelQueryRequest(string gridName)
    {
        GridName = gridName;
    }

    public string GridName { get; set; }
}