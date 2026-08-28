using System.Threading;
using System.Threading.Tasks;
using BackendCarcass.Identity;
using BackendCarcass.MasterData.Models;
using BackendCarcass.Repositories;
using BackendCarcassShared.Contracts.Errors;
using Microsoft.AspNetCore.Identity;
using SystemTools.Application.Abstractions.Messaging;
using SystemTools.SharedKernel;

namespace BackendCarcass.Application.UserRights.DeleteCurrentUser;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DeleteCurrentUserCommandHandler(UserManager<AppUser> userMgr, ICurrentUser currentUser)
    : ICommandHandler<DeleteCurrentUserRequestCommand>
{
    public async Task<Result> Handle(DeleteCurrentUserRequestCommand request, CancellationToken cancellationToken)
    {
        //ეს ერთგვარი ტესტია. თუ კოდი აქამდე მოვიდა, მიმდინარე მომხმარებელი ვალიდურია
        if (currentUser.Name != request.UserName)
        {
            return Result.Failure(UserRightsErrors.BadRequestFailedToDeleteUser);
        }

        var usersMdRepo = new UsersMdRepo(userMgr);
        AppUser? user = await userMgr.FindByNameAsync(request.UserName);
        //თუ არ მოიძებნა ასეთი, დავაბრუნოთ შეცდომა
        if (user == null)
        {
            return Result.Failure(UserRightsErrors.NoUserFound);
        }

        if (await usersMdRepo.Delete(user.Id))
        {
            return Result.Success();
        }

        return Result.Failure(UserRightsErrors.DeletionErrorUserCouldNotBeDeleted);
    }
}
