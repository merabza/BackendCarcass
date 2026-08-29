using BackendCarcass.Application.Repositories.Models;
using SystemTools.Application.Abstractions.Messaging;

//using MainMenuModel = BackendCarcass.Application.Repositories.Models.MainMenuModel;

namespace BackendCarcass.Application.UserRights.GetMainMenu;

public sealed class MainMenuRequestQuery : IQuery<MainMenuModel>;
