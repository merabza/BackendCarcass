namespace ServerCarcassData.Contracts.V1.Requests;

public sealed class SortFieldRequestPart
{
    public string? FieldName { get; set; }
    public bool Ascending { get; set; }
}