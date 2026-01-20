namespace BackendCarcass.Database.QueryModels;

public sealed class DataTypeTableModel
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DataTypeTableModel(int dtId, string dtTable)
    {
        DtId = dtId;
        DtTable = dtTable;
    }

    public string DtTable { get; set; }
    public int DtId { get; set; }
}