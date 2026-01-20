using System.Collections.Generic;
using BackendCarcass.Database.QueryModels;

namespace BackendCarcass.Repositories.Models;

public sealed class MainMenuModel
{
    public List<MenuGroupModel> MenuGroups { get; set; } = [];
}
