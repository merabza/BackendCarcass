namespace CarcassRepositories.Models;

public sealed class DataTypeToCrudRight
{
    public DataTypeToCrudRight(int dtId, string dtTable, string crtKey)
    {
        DtId = dtId;
        DtTable = dtTable;
        CrtKey = crtKey;
    }

    public int DtId { get; set; }
    public string DtTable { get; set; }
    public string CrtKey { get; set; }
}