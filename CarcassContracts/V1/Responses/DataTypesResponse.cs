namespace CarcassContracts.V1.Responses;

public sealed class DataTypesResponse
{
    public DataTypesResponse(string dtTable, string dtName, string dtNameNominative, string dtNameGenitive,
        string idFieldName, string? keyFieldName, string? nameFieldName)
    {
        DtTable = dtTable;
        DtName = dtName;
        DtNameNominative = dtNameNominative;
        DtNameGenitive = dtNameGenitive;
        IdFieldName = idFieldName;
        KeyFieldName = keyFieldName;
        NameFieldName = nameFieldName;
    }

    public string DtTable { get; set; }
    public string DtName { get; set; }
    public string DtNameNominative { get; set; }
    public string DtNameGenitive { get; set; }
    public string IdFieldName { get; set; }
    public string? KeyFieldName { get; set; }
    public string? NameFieldName { get; set; }
    public bool Create { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
}