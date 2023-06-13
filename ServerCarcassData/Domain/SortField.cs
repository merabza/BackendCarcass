namespace ServerCarcassData.Domain;

public sealed class SortField
{
    public SortField(bool ascending, string fieldName)
    {
        Ascending = ascending;
        FieldName = fieldName;
    }

    public string FieldName { get; set; }
    public bool Ascending { get; set; }
}