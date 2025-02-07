﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassContracts.Errors;
using CarcassMasterDataDom.Models;
using CarcassRepositories;
using MediatR;
using MessagingAbstractions;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.UserRights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DeleteCurrentUserCommandHandler : ICommandHandler<DeleteCurrentUserCommandRequest>
{
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteCurrentUserCommandHandler(UserManager<AppUser> userMgr)
    {
        _userMgr = userMgr;
    }

    public async Task<OneOf<Unit, IEnumerable<Err>>> Handle(DeleteCurrentUserCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserName = request.HttpRequest.HttpContext.User.Identity!.Name!;
        //ეს ერთგვარი ტესტია. თუ კოდი აქამდე მოვიდა, მიმდინარე მომხმარებელი ვალიდურია
        if (currentUserName != request.UserName)
            return new[] { UserRightsErrors.BadRequestFailedToDeleteUser };
        UsersMdRepo usersMdRepo = new(_userMgr);
        var user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
            return new[] { UserRightsErrors.NoUserFound };

        if (await usersMdRepo.Delete(user.Id))
            return new Unit();
        return new[] { UserRightsErrors.DeletionErrorUserCouldNotBeDeleted };
    }
}