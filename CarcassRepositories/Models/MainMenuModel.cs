using System.Collections.Generic;
using CarcassDb.QueryModels;

namespace CarcassRepositories.Models;

public sealed class MainMenuModel
{
    public List<MenuGroupModel> MenuGroups { get; set; } = new();
}