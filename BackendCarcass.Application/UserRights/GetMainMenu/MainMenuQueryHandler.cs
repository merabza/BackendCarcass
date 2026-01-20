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
public sealed class MainMenuQueryHandler : IQueryHandler<MainMenuRequestQuery, MainMenuModel>
{
    private readonly ICurrentUser _currentUser;
    private readonly IMenuRightsRepository _mdRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MainMenuQueryHandler(IMenuRightsRepository mdRepo, ICurrentUser currentUser)
    {
        _mdRepo = mdRepo;
        _currentUser = currentUser;
    }

    public async Task<OneOf<MainMenuModel, Err[]>> Handle(MainMenuRequestQuery request,
        CancellationToken cancellationToken)
    {
        MainMenuModel mainMenuModel = await _mdRepo.MainMenu(_currentUser.Name, cancellationToken);

        return mainMenuModel;
    }
}
