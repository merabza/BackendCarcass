using System.Collections.Generic;
using BackendCarcass.Domain.QueryModels;

namespace BackendCarcass.Application.Repositories.Models;

public sealed class MainMenuModel
{
    public List<MenuGroupModel> MenuGroups { get; set; } = [];
}
