using BackendCarcass.Domain;

namespace BackendCarcass.Application.MasterData;

public interface ISortedDataType : IDataType
{
    /// <summary>
    ///     სორტირების ჩანაწერის იდენტიფიკატორი
    /// </summary>
    int SortId { get; set; }
}
