using System.Collections.Generic;
using BackendCarcassDomain.Entities.QueryModels;

namespace BackendCarcass.Repositories.Models;

public sealed class MainMenuModel
{
    public List<MenuGroupModel> MenuGroups { get; set; } = [];
}
