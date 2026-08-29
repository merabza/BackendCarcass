using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Application.Identity;
using BackendCarcass.Application.Repositories;
using BackendCarcass.Application.Repositories.Models;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

//using ICurrentUser = BackendCarcass.Application.Identity.ICurrentUser;
//using IMenuRightsRepository = BackendCarcass.Application.Repositories.IMenuRightsRepository;
//using MainMenuModel = BackendCarcass.Application.Repositories.Models.MainMenuModel;

namespace BackendCarcass.Application.UserRights.GetMainMenu;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MainMenuQueryHandler(IMenuRightsRepository mdRepo, ICurrentUser currentUser)
    : IQueryHandler<MainMenuRequestQuery, MainMenuModel>
{
    public async Task<Result<MainMenuModel>> Handle(MainMenuRequestQuery request, CancellationToken cancellationToken)
    {
        MainMenuModel mainMenuModel = await mdRepo.MainMenu(currentUser.Name, cancellationToken);

        return mainMenuModel;
    }
}
