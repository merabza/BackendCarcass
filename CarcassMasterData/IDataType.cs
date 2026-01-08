namespace CarcassMasterData;

/// <summary>
///     ინტერფეისი <c>IDataType</c> გამოიყენება, მონაცემთა ტიპების და ძირითადი ინფორმაციის კლასებისთვის
/// </summary>
/// <remarks>
///     <c>ძირითადი ინფორმაცია</c>, ან <c>მონაცემთა ტიპი</c> არის ტერმინები და გამოიყენება სხვადასხვა რედაქტირებადი სიების
///     აღსანიშნავად
/// </remarks>
public interface IDataType
{
    /// <summary>
    ///     ჩანაწერის იდენტიფიკატორი
    /// </summary>
    int Id { get; set; }

    /// <summary>
    ///     ჩანაწერის უნიკალური გასაღები (ალტერნატიული გასაღები)
    /// </summary>
    string? Key { get; }

    /// <summary>
    ///     ჩანაწერის სახელი
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     მშობელი ჩანაწერის იდენტიფიკატორი
    /// </summary>
    int? ParentId { get; }

    /// <summary>
    ///     სხვა ჩანაწერიდან ინფორმაციის მიღება და მიმდინარე ჩანაწერში ჩაწერა
    /// </summary>
    bool UpdateTo(IDataType data);

    /// <summary>
    ///     მიმდინარე ჩანაწერის რედაქტირებადი ველების მიღება
    /// </summary>
    dynamic EditFields();
}