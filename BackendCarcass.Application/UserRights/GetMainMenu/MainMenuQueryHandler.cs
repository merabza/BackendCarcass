using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.Repositories;
using BackendCarcass.Repositories.Models;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.UserRights.GetMainMenu;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MainMenuQueryHandler(IMenuRightsRepository mdRepo, ICurrentUser currentUser)
    : IQueryHandler<MainMenuRequestQuery, MainMenuModel>
{
    public async Task<OneOf<MainMenuModel, Error[]>> Handle(MainMenuRequestQuery request,
        CancellationToken cancellationToken)
    {
        MainMenuModel mainMenuModel = await mdRepo.MainMenu(currentUser.Name, cancellationToken);

        return mainMenuModel;
    }
}
