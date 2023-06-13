﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarcassRepositories;
using CarcassRepositories.Models;
using MessagingAbstractions;
using OneOf;
using ServerCarcassMini.CommandRequests.UserRights;
using SystemToolsShared;

namespace ServerCarcassMini.Handlers.UserRights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class MainMenuQueryHandler : IQueryHandler<MainMenuQueryRequest, MainMenuModel>
{
    private readonly IMenuRightsRepository _mdRepo;

    public MainMenuQueryHandler(IMenuRightsRepository mdRepo)
    {
        _mdRepo = mdRepo;
    }

    public async Task<OneOf<MainMenuModel, IEnumerable<Err>>> Handle(MainMenuQueryRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserName = request.HttpRequest.HttpContext.User.Identity!.Name!;

        var mainMenuModel = await _mdRepo.MainMenu(currentUserName);

        return mainMenuModel;
    }
}