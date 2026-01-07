using CarcassRepositories.Models;
using MediatRMessagingAbstractions;

namespace Carcass.Application.UserRights.GetMainMenu;

public sealed class MainMenuRequestQuery : IQuery<MainMenuModel>;