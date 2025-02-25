using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.UserRights;
using CarcassIdentity;
using CarcassRepositories;
using CarcassRepositories.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.UserRights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MainMenuQueryHandler : IQueryHandler<MainMenuQueryRequest, MainMenuModel>
{
    private readonly ICurrentUser _currentUser;
    private readonly IMenuRightsRepository _mdRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MainMenuQueryHandler(IMenuRightsRepository mdRepo, ICurrentUser currentUser)
    {
        _mdRepo = mdRepo;
        _currentUser = currentUser;
    }

    public async Task<OneOf<MainMenuModel, IEnumerable<Err>>> Handle(MainMenuQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var mainMenuModel = await _mdRepo.MainMenu(_currentUser.Name, cancellationToken);

        return mainMenuModel;
    }
}