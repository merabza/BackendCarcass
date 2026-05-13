using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData.Models;
using BackendCarcass.Repositories;
using BackendCarcassShared.Contracts.Errors;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OneOf;
using SystemTools.MediatRMessagingAbstractions;
using SystemTools.SystemToolsShared.Errors;

namespace BackendCarcass.Application.UserRights.DeleteCurrentUser;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DeleteCurrentUserCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    : ICommandHandler<DeleteCurrentUserRequestCommand>
{
    public async Task<OneOf<Unit, Error[]>> Handle(DeleteCurrentUserRequestCommand request,
        CancellationToken cancellationToken)
    {
        //ეს ერთგვარი ტესტია. თუ კოდი აქამდე მოვიდა, მიმდინარე მომხმარებელი ვალიდურია
        if (currentUser.Name != request.UserName)
        {
            return new[] { UserRightsErrors.BadRequestFailedToDeleteUser };
        }

        var usersMdRepo = new UsersMdRepo(userMgr);
        AppUser? user = await userMgr.FindByNameAsync(request.UserName!);
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
