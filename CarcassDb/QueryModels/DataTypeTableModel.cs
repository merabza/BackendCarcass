namespace CarcassDb.QueryModels;

public sealed class DataTypeTableModel
{
    public DataTypeTableModel(int dtId, string dtTable)
    {
        DtId = dtId;
        DtTable = dtTable;
    }

    public string DtTable { get; set; }
    public int DtId { get; set; }
}