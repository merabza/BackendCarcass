namespace BackendCarcass.MasterData;

public interface ISortedDataType : IDataType
{
    /// <summary>
    ///     სორტირების ჩანაწერის იდენტიფიკატორი
    /// </summary>
    int SortId { get; set; }
}
