using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarcassDb.QueryModels;

public sealed class MenuGroupModel
{
    public MenuGroupModel(int mengId, string mengKey, string mengName, short sortId, string? mengIconName, bool hidden)
    {
        MengId = mengId;
        MengKey = mengKey;
        MengName = mengName;
        SortId = sortId;
        MengIconName = mengIconName;
        Hidden = hidden;
    }

    public int MengId { get; set; }
    public string MengKey { get; set; }
    public string MengName { get; set; }
    public short SortId { get; set; }
    public string? MengIconName { get; set; }
    public bool Hidden { get; set; }


    [NotMapped] public List<MenuItmModel> Menu { get; set; } = new();
}