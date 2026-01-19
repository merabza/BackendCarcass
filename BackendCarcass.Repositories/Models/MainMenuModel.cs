using System.Collections.Generic;
using Carcass.Database.QueryModels;

namespace CarcassRepositories.Models;

public sealed class MainMenuModel
{
    public List<MenuGroupModel> MenuGroups { get; set; } = [];
}