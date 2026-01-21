using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData.Models;
using BackendCarcass.Repositories;
using BackendCarcassContracts.Errors;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.UserRights.DeleteCurrentUser;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DeleteCurrentUserCommandHandler : ICommandHandler<DeleteCurrentUserRequestCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<AppUser> _userMgr;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteCurrentUserCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    {
        _userMgr = userMgr;
        _currentUser = currentUser;
    }

    public async Task<OneOf<Unit, Err[]>> Handle(DeleteCurrentUserRequestCommand request,
        CancellationToken cancellationToken)
    {
        //ეს ერთგვარი ტესტია. თუ კოდი აქამდე მოვიდა, მიმდინარე მომხმარებელი ვალიდურია
        if (_currentUser.Name != request.UserName)
        {
            return new[] { UserRightsErrors.BadRequestFailedToDeleteUser };
        }

        var usersMdRepo = new UsersMdRepo(_userMgr);
        AppUser? user = await _userMgr.FindByNameAsync(request.UserName!);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
        {
            return new[] { UserRightsErrors.NoUserFound };
        }

        if (await usersMdRepo.Delete(user.Id))
        {
            return new Unit();
        }

        return new[] { UserRightsErrors.DeletionErrorUserCouldNotBeDeleted };
    }
}
