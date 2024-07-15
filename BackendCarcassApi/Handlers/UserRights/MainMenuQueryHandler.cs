using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.UserRights;
using CarcassRepositories;
using CarcassRepositories.Models;
using MessagingAbstractions;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.UserRights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MainMenuQueryHandler : IQueryHandler<MainMenuQueryRequest, MainMenuModel>
{
    private readonly IMenuRightsRepository _mdRepo;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MainMenuQueryHandler(IMenuRightsRepository mdRepo)
    {
        _mdRepo = mdRepo;
    }

    public async Task<OneOf<MainMenuModel, IEnumerable<Err>>> Handle(MainMenuQueryRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserName = request.HttpRequest.HttpContext.User.Identity!.Name!;

        var mainMenuModel = await _mdRepo.MainMenu(currentUserName, cancellationToken);

        return mainMenuModel;
    }
}