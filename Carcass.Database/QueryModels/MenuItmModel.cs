namespace Carcass.Database.QueryModels;

public sealed class MenuItmModel
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public MenuItmModel(int menId, string menKey, string menLinkKey, string menName, string? menValue, int menGroupId,
        int sortId, string? menIconName)
    {
        MenId = menId;
        MenKey = menKey;
        MenLinkKey = menLinkKey;
        MenName = menName;
        MenValue = menValue;
        MenGroupId = menGroupId;
        SortId = sortId;
        MenIconName = menIconName;
    }

    public int MenId { get; set; }
    public string MenKey { get; set; }
    public string MenName { get; set; }
    public string? MenValue { get; set; }
    public int MenGroupId { get; set; }
    public int SortId { get; set; }
    public string MenLinkKey { get; set; }
    public string? MenIconName { get; set; }
    public bool Create { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public bool Confirm { get; set; }
}