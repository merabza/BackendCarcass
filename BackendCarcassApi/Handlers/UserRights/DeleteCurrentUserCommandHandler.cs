using System.Threading;
using System.Threading.Tasks;
using BackendCarcassApi.CommandRequests.UserRights;
using BackendCarcassContracts.Errors;
using CarcassIdentity;
using CarcassMasterDataDom.Models;
using CarcassRepositories;
using MediatR;
using MediatRMessagingAbstractions;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemToolsShared.Errors;

namespace BackendCarcassApi.Handlers.UserRights;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DeleteCurrentUserCommandHandler : ICommandHandler<DeleteCurrentUserCommandRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteCurrentUserCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    {
        _userMgr = userMgr;
        _currentUser = currentUser;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(DeleteCurrentUserCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        //ეს ერთგვარი ტესტია. თუ კოდი აქამდე მოვიდა, მიმდინარე მომხმარებელი ვალიდურია
        if (_currentUser.Name != request.UserName)
            return new[] { UserRightsErrors.BadRequestFailedToDeleteUser };
        var usersMdRepo = new UsersMdRepo(_userMgr);
        var user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
            return new[] { UserRightsErrors.NoUserFound };

        if (await usersMdRepo.Delete(user.Id))
            return new Unit();
        return new[] { UserRightsErrors.DeletionErrorUserCouldNotBeDeleted };
    }
}