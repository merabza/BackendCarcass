using System.Collections.Generic;

namespace ServerCarcassData.Contracts.V1.Requests;

public sealed class FilterSortObjectRequest
{
    //public int UserSequentialNumber { get; set; }
    public int? TabWindowId { get; set; }
    public string? TableName { get; set; }
    public List<SortFieldRequestPart> FilterByFields { get; set; } = new();
    public List<SortFieldRequestPart> SortByFields { get; set; } = new();
}