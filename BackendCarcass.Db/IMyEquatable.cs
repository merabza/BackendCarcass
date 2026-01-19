using BackendCarcass.MasterData;

namespace BackendCarcass.Db;

/// <summary>
///     ინტერფეისი <c>IMyEquatable</c> გამოიყენება, იმ შემთხვევაში, როცა გვჭირდება რომ ტიპს ჰქონდეს შედარების მეთოდი
///     <c>EqualsTo</c>
/// </summary>
public interface IMyEquatable
{
    /// <summary>
    ///     სხვა ჩანაწერის შედარება მიმდინარე ჩანაწერთან
    /// </summary>
    bool EqualsTo(IDataType data);
}
